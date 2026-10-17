namespace TheLicensePlateGame.Shared

open System

/// Event-sourcing domain events capturing state transitions for a game session.
type GameEvent =
    | PlateObserved of Observation
    | ObservationDeleted of Guid
    /// Attaches asynchronously-resolved GPS coordinates to an existing observation.
    | ObservationLocationRecorded of observationId: Guid * latitude: float * longitude: float
    | GameStarted of Game
    | GameUpdated of Game
    | GameEnded of Guid

/// Stamped event envelope ready for serialization and future server synchronization.
type StampedGameEvent = {
    Id: Guid
    GameId: Guid
    Timestamp: DateTimeOffset
    Event: GameEvent
}

/// Current state reconstructed by folding a sequence of stamped events.
type EventState = {
    /// All games ever started, keyed by game id (including completed games).
    Games: Map<Guid, Game>
    /// All live (non-deleted) observations across every game.
    Observations: Observation list
}

module Events =

    /// State before any events have been applied.
    let empty: EventState = { Games = Map.empty; Observations = [] }

    /// Wrap an event in a new stamped envelope.
    let stamp (gameId: Guid) (timestamp: DateTimeOffset) (event: GameEvent) : StampedGameEvent = {
        Id = Guid.NewGuid()
        GameId = gameId
        Timestamp = timestamp
        Event = event
    }

    /// Apply a single stamped event to the state. Pure and total: events that reference
    /// unknown games/observations are ignored so replaying a partial log never throws.
    let apply (state: EventState) (stamped: StampedGameEvent) : EventState =
        match stamped.Event with
        | PlateObserved obs -> { state with Observations = obs :: state.Observations }

        | ObservationDeleted obsId -> {
            state with
                Observations = state.Observations |> List.filter (fun o -> o.Id <> obsId)
          }

        | ObservationLocationRecorded(obsId, lat, lon) -> {
            state with
                Observations =
                    state.Observations
                    |> List.map (fun o ->
                        if o.Id = obsId then
                            {
                                o with
                                    Latitude = Some lat
                                    Longitude = Some lon
                            }
                        else
                            o)
          }

        | GameStarted game -> { state with Games = Map.add game.Id game state.Games }

        | GameUpdated updated ->
            match Map.tryFind updated.Id state.Games with
            | Some existing ->
                // Edits only change descriptive fields; lifecycle status is owned by GameEnded.
                let merged = {
                    existing with
                        Name = updated.Name
                        Notes = updated.Notes
                }
                { state with Games = Map.add merged.Id merged state.Games }
            | None -> state

        | GameEnded gameId ->
            match Map.tryFind gameId state.Games with
            | Some game when game.IsInProgress ->
                let ended = {
                    game with
                        Status = Completed stamped.Timestamp
                }
                { state with Games = Map.add gameId ended state.Games }
            | _ -> state

    /// Fold a sequence of events (in the given order) into a state.
    let fold (events: StampedGameEvent seq) : EventState = Seq.fold apply empty events

    /// Fold events after sorting them by timestamp (stable), for logs loaded from storage.
    let foldOrdered (events: StampedGameEvent seq) : EventState =
        events |> Seq.sortBy (fun e -> e.Timestamp) |> fold

    /// The most recently started game that is still in progress, if any.
    let activeGame (state: EventState) : Game option =
        state.Games
        |> Map.toList
        |> List.map snd
        |> List.filter (fun g -> g.IsInProgress)
        |> List.sortByDescending (fun g -> g.StartDate)
        |> List.tryHead

    /// Observations belonging to a specific game.
    let observationsFor (gameId: Guid) (state: EventState) : Observation list =
        state.Observations |> List.filter (fun o -> o.GameId = gameId)

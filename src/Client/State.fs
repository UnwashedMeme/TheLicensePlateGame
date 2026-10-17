namespace TheLicensePlateGame.Client

open System
open Elmish
open TheLicensePlateGame.Shared

module State =

    /// Helper to dismiss on-screen keyboards by blurring the active element.
    let private blurActiveElement () =
        try
            let activeEl = Browser.Dom.document.activeElement
            if not (isNull activeEl) then
                let htmlEl = activeEl :?> Browser.Types.HTMLElement
                if not (isNull htmlEl) then
                    htmlEl.blur ()
        with _ ->
            ()

    let private blurActiveInputCmd: Cmd<Msg> =
        Cmd.ofEffect (fun _ -> blurActiveElement ())

    /// Stamps domain events, folds them immediately into EventState (optimistic UI),
    /// and returns a command to persist them asynchronously in IndexedDB.
    let private recordEvents (events: (Guid * GameEvent) list) (model: Model) : Model * Cmd<Msg> =
        let now = DateTimeOffset.UtcNow
        let stampedEvents = events |> List.map (fun (gId, ev) -> Events.stamp gId now ev)

        let nextEventState = stampedEvents |> List.fold Events.apply model.EventState

        let nextActiveGame =
            Events.activeGame nextEventState |> Option.defaultValue model.ActiveGame

        let nextObservations = Events.observationsFor nextActiveGame.Id nextEventState

        let nextModel = {
            model with
                EventState = nextEventState
                ActiveGame = nextActiveGame
                Observations = nextObservations
        }

        let persistCmd = Storage.appendEvents stampedEvents EventsPersisted
        nextModel, persistCmd

    /// Initialize the default in-memory game state.
    let init () : Model * Cmd<Msg> =
        let initialGame = Game.New()
        let initialEventState = {
            Events.empty with
                Games = Map.ofList [ initialGame.Id, initialGame ]
        }

        let initialModel: Model = {
            ActiveGame = initialGame
            Observations = []
            Scopes = Scope.currentlyUsed |> List.map Scope.ofTag
            SearchQuery = ""
            HideObserved = false
            ActiveModal = None
            EventState = initialEventState
        }

        initialModel, Cmd.none

    /// Pure MVU update function handling all application transitions.
    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | EventsLoaded(Ok events) ->
            if List.isEmpty events then
                model, Cmd.none
            else
                let foldedState = Events.foldOrdered events
                match Events.activeGame foldedState with
                | Some active ->
                    let observations = Events.observationsFor active.Id foldedState
                    {
                        model with
                            EventState = foldedState
                            ActiveGame = active
                            Observations = observations
                    },
                    Cmd.none
                | None -> { model with EventState = foldedState }, Cmd.none

        | EventsLoaded(Error err) ->
            Browser.Dom.console.error ($"Failed to load events from IndexedDB: {err}")
            model, Cmd.none

        | EventsPersisted(Ok()) -> model, Cmd.none

        | EventsPersisted(Error err) ->
            Browser.Dom.console.error ($"Failed to persist events to IndexedDB: {err}")
            model, Cmd.none

        | PlateTapped jurisdictionId ->
            let newObservation: Observation = {
                Id = Guid.NewGuid()
                GameId = model.ActiveGame.Id
                JurisdictionId = jurisdictionId
                Timestamp = DateTimeOffset.UtcNow
                Latitude = None
                Longitude = None
            }

            let nextModel, persistCmd =
                recordEvents [ model.ActiveGame.Id, PlateObserved newObservation ] model

            { nextModel with SearchQuery = "" }, Cmd.batch [ persistCmd; blurActiveInputCmd ]

        | OpenObservationDetails jurisdictionId ->
            let count = Model.countForJurisdiction jurisdictionId model
            if count >= 1 then
                {
                    model with
                        ActiveModal = Some(ObservationDetails jurisdictionId)
                },
                blurActiveInputCmd
            else
                model, Cmd.none

        | SearchChanged query -> { model with SearchQuery = query }, Cmd.none

        | ClearSearch -> { model with SearchQuery = "" }, Cmd.none

        | ToggleScope scope ->
            let updatedScopes =
                List.map (fun s -> if s.Tag = scope.Tag then Scope.toggle s else s) model.Scopes
            { model with Scopes = updatedScopes }, Cmd.none

        | ToggleHideObserved ->
            {
                model with
                    HideObserved = not model.HideObserved
            },
            Cmd.none

        | OpenMenu ->
            {
                model with
                    ActiveModal = Some HamburgerMenu
            },
            Cmd.none

        | EditGame ->
            let newModal = GameModal(GameModal.EditGame model.ActiveGame)
            {
                model with
                    ActiveModal = Some newModal
            },
            Cmd.none

        | NewGame ->
            let newModal = GameModal(GameModal.NewGame(Game.New()))
            {
                model with
                    ActiveModal = Some newModal
            },
            Cmd.none

        | CloseModal -> { model with ActiveModal = None }, Cmd.none

        | GameSaved updatedGame ->
            let nextModel, persistCmd =
                recordEvents [ updatedGame.Id, GameUpdated updatedGame ] model

            { nextModel with ActiveModal = None }, persistCmd

        | NewGameStarted newGame ->
            let events = [
                model.ActiveGame.Id, GameEnded model.ActiveGame.Id
                newGame.Id, GameStarted newGame
            ]
            let nextModel, persistCmd = recordEvents events model

            {
                nextModel with
                    SearchQuery = ""
                    ActiveModal = None
            },
            persistCmd

        | DeleteObservation id ->
            let nextModel, persistCmd =
                recordEvents [ model.ActiveGame.Id, ObservationDeleted id ] model

            // If an observation details modal is currently open, check if any entries remain
            let nextModal =
                match nextModel.ActiveModal with
                | Some(ObservationDetails jId) ->
                    let hasRemaining =
                        nextModel.Observations |> List.exists (fun o -> o.JurisdictionId = jId)
                    if hasRemaining then Some(ObservationDetails jId) else None
                | other -> other

            {
                nextModel with
                    ActiveModal = nextModal
            },
            persistCmd

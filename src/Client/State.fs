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

    /// Initialize the default in-memory game state.
    let init () : Model * Cmd<Msg> =
        let now = DateTimeOffset.UtcNow
        let defaultGameName = sprintf "Trip - %s" (now.ToString("MMM d, yyyy"))
        let initialGame: Game = {
            Id = Guid.NewGuid()
            Name = defaultGameName
            Notes = None
            StartDate = now
            Status = InProgress
        }

        let initialModel: Model = {
            ActiveGame = initialGame
            Observations = []
            Scopes = Scope.currentlyUsed |> List.map Scope.ofTag
            SearchQuery = ""
            HideObserved = false
            ActiveModal = None
        }

        initialModel, Cmd.none

    /// Pure MVU update function handling all application transitions.
    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | PlateTapped jurisdictionId ->
            let newObservation: Observation = {
                Id = Guid.NewGuid()
                GameId = model.ActiveGame.Id
                JurisdictionId = jurisdictionId
                Timestamp = DateTimeOffset.UtcNow
                Latitude = None
                Longitude = None
            }

            let nextModel = {
                model with
                    Observations = newObservation :: model.Observations
                    SearchQuery = ""
            }

            nextModel, blurActiveInputCmd

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

        | OpenModal modal -> { model with ActiveModal = Some modal }, Cmd.none

        | CloseModal -> { model with ActiveModal = None }, Cmd.none

        | EditGameNameChanged newName ->
            match model.ActiveModal with
            | Some(EditGameInfo(_, notes)) ->
                {
                    model with
                        ActiveModal = Some(EditGameInfo(newName, notes))
                },
                Cmd.none
            | _ -> model, Cmd.none

        | EditGameNotesChanged newNotes ->
            match model.ActiveModal with
            | Some(EditGameInfo(name, _)) ->
                {
                    model with
                        ActiveModal = Some(EditGameInfo(name, newNotes))
                },
                Cmd.none
            | _ -> model, Cmd.none

        | SaveGameInfo ->
            match model.ActiveModal with
            | Some(EditGameInfo(name, notes)) ->
                let cleanedName =
                    if String.IsNullOrWhiteSpace name then
                        model.ActiveGame.Name
                    else
                        name.Trim()
                let cleanedNotes =
                    if String.IsNullOrWhiteSpace notes then
                        None
                    else
                        Some(notes.Trim())
                let updatedGame = {
                    model.ActiveGame with
                        Name = cleanedName
                        Notes = cleanedNotes
                }
                {
                    model with
                        ActiveGame = updatedGame
                        ActiveModal = None
                },
                Cmd.none
            | _ -> model, Cmd.none

        | NewGameNameChanged newName ->
            match model.ActiveModal with
            | Some(NewGameConfirm(_, notes)) ->
                {
                    model with
                        ActiveModal = Some(NewGameConfirm(newName, notes))
                },
                Cmd.none
            | _ -> model, Cmd.none

        | NewGameNotesChanged newNotes ->
            match model.ActiveModal with
            | Some(NewGameConfirm(name, _)) ->
                {
                    model with
                        ActiveModal = Some(NewGameConfirm(name, newNotes))
                },
                Cmd.none
            | _ -> model, Cmd.none

        | ConfirmNewGame ->
            match model.ActiveModal with
            | Some(NewGameConfirm(name, notes)) ->
                let now = DateTimeOffset.UtcNow
                let fallbackName = sprintf "Trip - %s" (now.ToString("MMM d, yyyy"))
                let newGameTitle =
                    if String.IsNullOrWhiteSpace name then
                        fallbackName
                    else
                        name.Trim()
                let cleanedNotes =
                    if String.IsNullOrWhiteSpace notes then
                        None
                    else
                        Some(notes.Trim())

                let newGame: Game = {
                    Id = Guid.NewGuid()
                    Name = newGameTitle
                    Notes = cleanedNotes
                    StartDate = now
                    Status = InProgress
                }

                let freshModel = {
                    model with
                        ActiveGame = newGame
                        Observations = []
                        SearchQuery = ""
                        ActiveModal = None
                }
                freshModel, Cmd.none
            | _ -> model, Cmd.none

        | DeleteObservation id ->
            let updatedObservations = model.Observations |> List.filter (fun o -> o.Id <> id)

            // If an observation details modal is currently open, check if any entries remain
            let nextModal =
                match model.ActiveModal with
                | Some(ObservationDetails jId) ->
                    let hasRemaining =
                        updatedObservations |> List.exists (fun o -> o.JurisdictionId = jId)
                    if hasRemaining then Some(ObservationDetails jId) else None
                | other -> other

            {
                model with
                    Observations = updatedObservations
                    ActiveModal = nextModal
            },
            Cmd.none

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
        let initialGame = Game.New()

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

        | GameModalMsg childMsg ->
            match model.ActiveModal with
            | Some(GameModal childModel) ->
                let updatedChild, cmd, outMsg = GameModal.update childMsg childModel
                let nextModel =
                    match outMsg with
                    | Some(GameModal.SaveGame updatedGame) -> {
                        model with
                            ActiveGame = updatedGame
                            ActiveModal = None
                      }
                    | Some(GameModal.StartNewGame newGame) ->
                        // TODO; I think this is where we set the EndDate of the old game as that gets persisted.
                        {
                            model with
                                ActiveGame = newGame
                                Observations = []
                                SearchQuery = ""
                                ActiveModal = None
                        }
                    | Some GameModal.Cancelled -> { model with ActiveModal = None }
                    | None -> {
                        model with
                            ActiveModal = Some(GameModal updatedChild)
                      }
                nextModel, Cmd.map GameModalMsg cmd
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

namespace TheLicensePlateGame.Client

open System
open Browser.Types
open Feliz
open Feliz.UseElmish
open TheLicensePlateGame.Shared

module View =


    // =========================================================================
    // Header Component
    // =========================================================================

    let private headerView (model: Model) (dispatch: Msg -> unit) =
        Html.header [
            prop.className "app-header"
            prop.children [
                Html.div [
                    prop.className "header-content"
                    prop.children [
                        // Left: Active Game Title (Tappable to edit)
                        Html.div [
                            prop.className "header-left"
                            prop.children [
                                Html.button [
                                    prop.className "game-title-button"
                                    prop.ariaLabel "Edit game title and notes"
                                    prop.onClick (fun _ -> dispatch EditGame)
                                    prop.children [
                                        Html.span [ prop.className "game-title"; prop.text model.ActiveGame.Name ]
                                        Icons.edit ()
                                    ]
                                ]
                            ]
                        ]

                        // Right: Live Progress Counter & Hamburger Menu
                        Html.div [
                            prop.className "header-right"
                            prop.children [
                                Html.div [
                                    prop.className "progress-counter"
                                    prop.ariaLabel "Collection progress"
                                    prop.text (Model.progressText model.Scopes model)
                                ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.ariaLabel "Open navigation menu"
                                    prop.onClick (fun _ -> dispatch OpenMenu)
                                    prop.children [ Icons.hamburger () ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    // =========================================================================
    // Controls: Scope Tabs & Search Input
    // =========================================================================

    let private controlsView (model: Model) (dispatch: Msg -> unit) =
        Html.section [
            prop.className "controls-section"
            prop.children [
                // Scope Filter Toggles
                Html.div [
                    prop.className "scope-tabs"
                    prop.role "group"
                    prop.ariaLabel "Jurisdiction filters"
                    prop.children (
                        model.Scopes
                        |> List.map (fun scope ->

                            Html.button [
                                let isActive = scope.Active
                                prop.type' "button"

                                prop.className (if isActive then "scope-pill is-active" else "scope-pill")

                                prop.ariaPressed isActive
                                prop.onClick (fun _ -> dispatch (ToggleScope scope))

                                prop.children [
                                    Html.span scope.Tag.UiFriendlyName
                                    Html.span [ prop.className "scope-pill-count"; prop.text (string scope.Count) ]
                                ]
                            ]

                        )
                    )
                ]

                // Search Bar
                Html.div [
                    prop.className "search-box-wrapper"
                    prop.children [
                        Icons.search ()
                        Html.input [
                            prop.className "search-input"
                            prop.type' "search"
                            prop.placeholder "Search state, province, or postal code..."
                            prop.ariaLabel "Search jurisdictions"
                            prop.value model.SearchQuery
                            prop.onChange (SearchChanged >> dispatch)
                        ]
                        if not (String.IsNullOrEmpty model.SearchQuery) then
                            Html.button [
                                prop.className "search-clear-button"
                                prop.ariaLabel "Clear search"
                                prop.onClick (fun _ -> dispatch ClearSearch)
                                prop.children [ Icons.close () ]
                            ]
                    ]
                ]
            ]
        ]

    // =========================================================================
    // Game Board: Jurisdiction Plate Grid
    // =========================================================================

    [<ReactComponent>]
    let private PlateCard (jurisdiction: Jurisdiction) (count: int) (dispatch: Msg -> unit) =
        let isObserved = count > 0
        let timerRef = React.useRef<float option>(None)
        let startPosRef = React.useRef<float * float>((0.0, 0.0))
        let isLongPressRef = React.useRef<bool>(false)
        let hasMovedRef = React.useRef<bool>(false)

        let cancelTimer () =
            match timerRef.current with
            | Some t ->
                Browser.Dom.window.clearTimeout t
                timerRef.current <- None
            | None -> ()

        React.useEffect ((fun () -> cancelTimer), [||])

        Html.button [
            prop.key jurisdiction.Id
            prop.className (
                if isObserved then
                    "plate-card is-observed"
                else
                    "plate-card"
            )
            prop.ariaLabel (sprintf "%s (%s), %d observed" jurisdiction.Name jurisdiction.Code count)
            prop.onContextMenu (fun e -> e.preventDefault ())
            prop.onPointerDown (fun (e: Browser.Types.PointerEvent) ->
                if e.button = 0.0 then
                    cancelTimer ()
                    isLongPressRef.current <- false
                    hasMovedRef.current <- false
                    startPosRef.current <- (e.clientX, e.clientY)

                    let timeoutId =
                        Browser.Dom.window.setTimeout (
                            (fun () ->
                                timerRef.current <- None
                                isLongPressRef.current <- true

                                if count >= 1 then
                                    dispatch (OpenObservationDetails jurisdiction.Id)),
                            500
                        )

                    timerRef.current <- Some timeoutId)
            prop.onPointerMove (fun (e: Browser.Types.PointerEvent) ->
                let startX, startY = startPosRef.current
                let dx = e.clientX - startX
                let dy = e.clientY - startY

                if (dx * dx + dy * dy) > 100.0 then
                    cancelTimer ()
                    hasMovedRef.current <- true)
            prop.onPointerUp (fun _ -> cancelTimer ())
            prop.onPointerCancel (fun _ ->
                cancelTimer ()
                hasMovedRef.current <- false
                isLongPressRef.current <- false)
            prop.onClick (fun _ ->
                if isLongPressRef.current then
                    isLongPressRef.current <- false
                elif hasMovedRef.current then
                    hasMovedRef.current <- false
                else
                    dispatch (PlateTapped jurisdiction.Id))
            prop.children [
                // Top header: Postal code on left, Count badge on right
                Html.div [
                    prop.className "plate-card-header"
                    prop.children [
                        Html.span [ prop.className "plate-card-code"; prop.text jurisdiction.Code ]
                        Html.span [ prop.className "plate-card-badge"; prop.text (string count) ]
                    ]
                ]

                // Centered name in lower two-thirds
                Html.div [
                    prop.className "plate-card-body"
                    prop.children [ Html.span [ prop.className "plate-card-name"; prop.text jurisdiction.Name ] ]
                ]
            ]
        ]

    let private plateGridView (model: Model) (dispatch: Msg -> unit) =
        let scopedJurisdictions = Model.jurisdictionsForScopes model.Scopes
        let searchFiltered = Search.filter model.SearchQuery scopedJurisdictions

        let displayedJurisdictions =
            if model.HideObserved then
                searchFiltered |> List.filter (fun j -> not (Model.isObserved j.Id model))
            else
                searchFiltered

        if List.isEmpty displayedJurisdictions then
            Html.div [
                prop.className "empty-state"
                prop.children [
                    Html.p [ prop.className "empty-state-title"; prop.text "No jurisdictions found" ]
                    Html.p [
                        prop.text (
                            if model.HideObserved then
                                "All jurisdictions in this view are collected! Switch filters or toggle 'Show Observed'."
                            else
                                "No state or province matched your search query."
                        )
                    ]
                ]
            ]
        else
            Html.div [
                prop.className "plate-grid"
                prop.children [
                    for j in displayedJurisdictions do
                        let count = Model.countForJurisdiction j.Id model
                        PlateCard j count dispatch
                ]
            ]

    // =========================================================================
    // Hamburger Navigation Drawer
    // =========================================================================

    let private drawerView (model: Model) (dispatch: Msg -> unit) =
        match model.ActiveModal with
        | Some HamburgerMenu ->
            React.Fragment [
                Html.div [
                    prop.className "drawer-backdrop"
                    prop.onClick (fun _ -> dispatch CloseModal)
                ]
                Html.aside [
                    prop.className "drawer-panel"
                    prop.children [
                        Html.div [
                            prop.className "modal-header"
                            prop.children [
                                Html.h2 [ prop.className "modal-title"; prop.text "Menu" ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.ariaLabel "Close menu"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.children [ Icons.close () ]
                                ]
                            ]
                        ]

                        Html.nav [
                            prop.className "drawer-nav"
                            prop.children [
                                // Toggle Hide/Show Observed
                                Html.button [
                                    prop.className "drawer-item"
                                    prop.onClick (fun _ ->
                                        dispatch ToggleHideObserved
                                        dispatch CloseModal)
                                    prop.text (
                                        if model.HideObserved then
                                            "Show Observed States"
                                        else
                                            "Hide Observed States"
                                    )
                                ]

                                // Edit Game Info
                                Html.button [
                                    prop.className "drawer-item"
                                    prop.onClick (fun _ -> dispatch EditGame)
                                    prop.text "Edit Game Info"
                                ]

                                // New Game
                                Html.button [
                                    prop.className "drawer-item"
                                    prop.onClick (fun _ -> dispatch NewGame)
                                    prop.text "New Game"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        | _ -> Html.none

    // =========================================================================
    // Modals: Edit Game Info, New Game Confirm, Observation Details
    // =========================================================================


    let private modalsView (model: Model) (dispatch: Msg -> unit) =
        match model.ActiveModal with
        | Some(GameModal gm) ->
            GameModal.Modal gm (GameSaved >> dispatch) (NewGameStarted >> dispatch) (fun () -> dispatch CloseModal)
        | Some(ObservationDetails jId) ->
            let observations = Model.observationsForJurisdiction jId model
            ObservationModal.view jId observations (DeleteObservation >> dispatch) (fun () -> dispatch CloseModal)
        | _ -> Html.none

    // =========================================================================
    // Root Application View
    // =========================================================================

    let root (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "app-shell"
            prop.children [
                headerView model dispatch
                Html.main [
                    prop.className "main-content"
                    prop.children [ controlsView model dispatch; plateGridView model dispatch ]
                ]
                drawerView model dispatch
                modalsView model dispatch
            ]
        ]

    /// Root React component: runs the application's Elmish loop via `React.useElmish`.
    [<ReactComponent>]
    let App () =
        let model, dispatch = React.useElmish (State.init, State.update, [||])
        root model dispatch

namespace TheLicensePlateGame.Client

open System
open Browser.Types
open Feliz
open TheLicensePlateGame.Shared

module View =

    // =========================================================================
    // SVG Icons
    // =========================================================================

    module Icons =
        let hamburger () =
            Svg.svg [
                svg.viewBox (0, 0, 24, 24)
                svg.fill "none"
                svg.stroke "currentColor"
                svg.strokeWidth 2
                svg.children [
                    Svg.line [ svg.x1 3; svg.y1 6; svg.x2 21; svg.y2 6 ]
                    Svg.line [ svg.x1 3; svg.y1 12; svg.x2 21; svg.y2 12 ]
                    Svg.line [ svg.x1 3; svg.y1 18; svg.x2 21; svg.y2 18 ]
                ]
            ]

        let search () =
            Svg.svg [
                svg.className "search-icon"
                svg.viewBox (0, 0, 24, 24)
                svg.fill "none"
                svg.stroke "currentColor"
                svg.strokeWidth 2
                svg.children [
                    Svg.circle [ svg.cx 11; svg.cy 11; svg.r 8 ]
                    Svg.line [ svg.x1 21; svg.y1 21; svg.x2 16.65; svg.y2 16.65 ]
                ]
            ]

        let close () =
            Svg.svg [
                svg.viewBox (0, 0, 24, 24)
                svg.fill "none"
                svg.stroke "currentColor"
                svg.strokeWidth 2
                svg.children [
                    Svg.line [ svg.x1 18; svg.y1 6; svg.x2 6; svg.y2 18 ]
                    Svg.line [ svg.x1 6; svg.y1 6; svg.x2 18; svg.y2 18 ]
                ]
            ]

        let edit () =
            Svg.svg [
                svg.className "game-title-edit-icon"
                svg.viewBox (0, 0, 24, 24)
                svg.fill "none"
                svg.stroke "currentColor"
                svg.strokeWidth 2
                svg.children [
                    Svg.path [ svg.d "M12 20h9" ]
                    Svg.path [ svg.d "M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" ]
                ]
            ]

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
                                    prop.onClick (fun _ ->
                                        let currentName = model.ActiveGame.Name
                                        let currentNotes = defaultArg model.ActiveGame.Notes ""
                                        dispatch (OpenModal (EditGameInfo (currentName, currentNotes)))
                                    )
                                    prop.children [
                                        Html.span [
                                            prop.className "game-title"
                                            prop.text model.ActiveGame.Name
                                        ]
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
                                    prop.text (Model.progressText model.SelectedTab model)
                                ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.ariaLabel "Open navigation menu"
                                    prop.onClick (fun _ -> dispatch (OpenModal HamburgerMenu))
                                    prop.children [
                                        Icons.hamburger ()
                                    ]
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
                // Scope Filter Tabs
                Html.div [
                    prop.className "scope-tabs"
                    prop.role "tablist"
                    prop.children [
                        // US Tab
                        Html.button [
                            let isActive = model.SelectedTab = US
                            prop.className (if isActive then "scope-pill is-active" else "scope-pill")
                            prop.role "tab"
                            prop.ariaSelected isActive
                            prop.onClick (fun _ -> dispatch (TabSelected US))
                            prop.children [
                                Html.span "US States"
                                Html.span [
                                    prop.className "scope-pill-count"
                                    prop.text "51"
                                ]
                            ]
                        ]

                        // Canadian Provinces Tab
                        Html.button [
                            let isActive = model.SelectedTab = Canada
                            prop.className (if isActive then "scope-pill is-active" else "scope-pill")
                            prop.role "tab"
                            prop.ariaSelected isActive
                            prop.onClick (fun _ -> dispatch (TabSelected Canada))
                            prop.children [
                                Html.span "Canadian Provinces"
                                Html.span [
                                    prop.className "scope-pill-count"
                                    prop.text "10"
                                ]
                            ]
                        ]

                        // All Tab
                        Html.button [
                            let isActive = model.SelectedTab = All
                            prop.className (if isActive then "scope-pill is-active" else "scope-pill")
                            prop.role "tab"
                            prop.ariaSelected isActive
                            prop.onClick (fun _ -> dispatch (TabSelected All))
                            prop.children [
                                Html.span "All"
                                Html.span [
                                    prop.className "scope-pill-count"
                                    prop.text "61"
                                ]
                            ]
                        ]
                    ]
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
                                prop.children [
                                    Icons.close ()
                                ]
                            ]
                    ]
                ]
            ]
        ]

    // =========================================================================
    // Game Board: Jurisdiction Plate Grid
    // =========================================================================

    let private plateGridView (model: Model) (dispatch: Msg -> unit) =
        let scopedJurisdictions = Model.jurisdictionsForScope model.SelectedTab
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
                    Html.p [
                        prop.className "empty-state-title"
                        prop.text "No jurisdictions found"
                    ]
                    Html.p [
                        prop.text (
                            if model.HideObserved then
                                "All jurisdictions in this view are collected! Switch tabs or toggle 'Show Observed'."
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
                        let isObserved = count > 0

                        Html.button [
                            prop.key j.Id
                            prop.className (if isObserved then "plate-card is-observed" else "plate-card")
                            prop.ariaLabel (
                                sprintf "%s (%s), %d observed" j.Name j.Code count
                            )
                            prop.onClick (fun _ ->
                                dispatch (PlateTapped j.Id)
                            )
                            prop.children [
                                // Top header: Postal code on left, Count badge on right
                                Html.div [
                                    prop.className "plate-card-header"
                                    prop.children [
                                        Html.span [
                                            prop.className "plate-card-code"
                                            prop.text j.Code
                                        ]
                                        Html.span [
                                            prop.className "plate-card-badge"
                                            prop.text (string count)
                                        ]
                                    ]
                                ]

                                // Centered name in lower two-thirds
                                Html.div [
                                    prop.className "plate-card-body"
                                    prop.children [
                                        Html.span [
                                            prop.className "plate-card-name"
                                            prop.text j.Name
                                        ]
                                    ]
                                ]
                            ]
                        ]
                ]
            ]

    // =========================================================================
    // Hamburger Navigation Drawer
    // =========================================================================

    let private drawerView (model: Model) (dispatch: Msg -> unit) =
        match model.ActiveModal with
        | Some HamburgerMenu ->
            React.fragment [
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
                                Html.h2 [
                                    prop.className "modal-title"
                                    prop.text "Menu"
                                ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.ariaLabel "Close menu"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.children [
                                        Icons.close ()
                                    ]
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
                                        dispatch CloseModal
                                    )
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
                                    prop.onClick (fun _ ->
                                        let currentName = model.ActiveGame.Name
                                        let currentNotes = defaultArg model.ActiveGame.Notes ""
                                        dispatch (OpenModal (EditGameInfo (currentName, currentNotes)))
                                    )
                                    prop.text "Edit Game Info"
                                ]

                                // New Game
                                Html.button [
                                    prop.className "drawer-item"
                                    prop.onClick (fun _ ->
                                        let defaultNewName = sprintf "Trip - %s" (DateTimeOffset.UtcNow.ToString("MMM d, yyyy"))
                                        dispatch (OpenModal (NewGameConfirm (defaultNewName, "")))
                                    )
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

    let private editGameModal (name: string) (notes: string) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> dispatch CloseModal)
            prop.children [
                Html.div [
                    prop.className "modal-card"
                    prop.onClick (fun e -> e.stopPropagation ())
                    prop.children [
                        Html.div [
                            prop.className "modal-header"
                            prop.children [
                                Html.h2 [ prop.className "modal-title"; prop.text "Edit Game Info" ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.children [ Icons.close () ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-body"
                            prop.children [
                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "Game Name" ]
                                        Html.input [
                                            prop.className "form-input"
                                            prop.value name
                                            prop.onChange (EditGameNameChanged >> dispatch)
                                            prop.placeholder "e.g. 2026 Summer Roadtrip"
                                        ]
                                    ]
                                ]

                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "Notes (optional)" ]
                                        Html.textarea [
                                            prop.className "form-textarea"
                                            prop.value notes
                                            prop.onChange (EditGameNotesChanged >> dispatch)
                                            prop.placeholder "Add details or goals for this trip..."
                                        ]
                                    ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-footer"
                            prop.children [
                                Html.button [
                                    prop.className "btn btn-secondary"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> dispatch SaveGameInfo)
                                    prop.text "Save"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private newGameModal (name: string) (notes: string) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> dispatch CloseModal)
            prop.children [
                Html.div [
                    prop.className "modal-card"
                    prop.onClick (fun e -> e.stopPropagation ())
                    prop.children [
                        Html.div [
                            prop.className "modal-header"
                            prop.children [
                                Html.h2 [ prop.className "modal-title"; prop.text "Start New Game" ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.children [ Icons.close () ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-body"
                            prop.children [
                                Html.p [
                                    prop.className "form-label"
                                    prop.text "Starting a new game concludes your active session. All historical observations are preserved."
                                ]
                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "New Game Name" ]
                                        Html.input [
                                            prop.className "form-input"
                                            prop.value name
                                            prop.onChange (NewGameNameChanged >> dispatch)
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "Notes (optional)" ]
                                        Html.textarea [
                                            prop.className "form-textarea"
                                            prop.value notes
                                            prop.onChange (NewGameNotesChanged >> dispatch)
                                            prop.placeholder "Trip notes..."
                                        ]
                                    ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-footer"
                            prop.children [
                                Html.button [
                                    prop.className "btn btn-secondary"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> dispatch ConfirmNewGame)
                                    prop.text "Start Game"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private observationDetailsModal (jurisdictionId: string) (model: Model) (dispatch: Msg -> unit) =
        let jurisdictionOpt = Jurisdictions.tryFind jurisdictionId
        let title =
            match jurisdictionOpt with
            | Some j -> sprintf "%s (%s)" j.Name j.Code
            | None -> jurisdictionId

        let observations = Model.observationsForJurisdiction jurisdictionId model

        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> dispatch CloseModal)
            prop.children [
                Html.div [
                    prop.className "modal-card"
                    prop.onClick (fun e -> e.stopPropagation ())
                    prop.children [
                        Html.div [
                            prop.className "modal-header"
                            prop.children [
                                Html.h2 [ prop.className "modal-title"; prop.text title ]
                                Html.button [
                                    prop.className "icon-button"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.children [ Icons.close () ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-body"
                            prop.children [
                                Html.ul [
                                    prop.className "observation-list"
                                    prop.children [
                                        for obs in observations do
                                            Html.li [
                                                prop.key (string obs.Id)
                                                prop.className "observation-item"
                                                prop.children [
                                                    Html.span [
                                                        prop.className "observation-timestamp"
                                                        prop.text (obs.Timestamp.ToLocalTime().ToString("MMM d, h:mm tt"))
                                                    ]
                                                    Html.button [
                                                        prop.className "observation-delete-button"
                                                        prop.ariaLabel "Delete observation"
                                                        prop.onClick (fun _ -> dispatch (DeleteObservation obs.Id))
                                                        prop.children [ Icons.close () ]
                                                    ]
                                                ]
                                            ]
                                    ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-footer"
                            prop.children [
                                Html.button [
                                    prop.className "btn btn-secondary"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                    prop.text "Close"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private modalsView (model: Model) (dispatch: Msg -> unit) =
        match model.ActiveModal with
        | Some (EditGameInfo (name, notes)) ->
            editGameModal name notes dispatch
        | Some (NewGameConfirm (name, notes)) ->
            newGameModal name notes dispatch
        | Some (ObservationDetails jId) ->
            observationDetailsModal jId model dispatch
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
                    prop.children [
                        controlsView model dispatch
                        plateGridView model dispatch
                    ]
                ]
                drawerView model dispatch
                modalsView model dispatch
            ]
        ]

namespace TheLicensePlateGame.Client

open System
open Elmish
open Feliz
open Browser.Types


open TheLicensePlateGame.Shared

module GameModal =

    type Msg =
        | GameNameChanged of string
        | GameNotesChanged of string
        | Save
        | Cancel

    type Model =
        | EditGame of Game: Game
        | NewGame of Game: Game

    type OutMsg =
        | SaveGame of Game
        | StartNewGame of Game
        | Cancelled

    let private cleanGame (game: Game) (fallbackName: string) : Game =
        let cleanedName =
            if String.IsNullOrWhiteSpace game.Name then
                fallbackName
            else
                game.Name.Trim()

        let cleanedNotes =
            match game.Notes with
            | Some notes when not (String.IsNullOrWhiteSpace notes) -> Some(notes.Trim())
            | _ -> None

        {
            game with
                Name = cleanedName
                Notes = cleanedNotes
        }

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * OutMsg option =
        match msg with
        | GameNameChanged name ->
            let nextModel =
                match model with
                | EditGame game -> EditGame { game with Name = name }
                | NewGame game -> NewGame { game with Name = name }

            nextModel, Cmd.none, None

        | GameNotesChanged notes ->
            let nextModel =
                match model with
                | EditGame game -> EditGame { game with Notes = Some notes }
                | NewGame game -> NewGame { game with Notes = Some notes }

            nextModel, Cmd.none, None

        | Save ->
            match model with
            | EditGame game ->
                let updated = cleanGame game game.Name
                model, Cmd.none, Some(SaveGame updated)
            | NewGame game ->
                let fallback = sprintf "Trip - %s" (DateTimeOffset.Now.ToString("MMM d, yyyy"))
                let updated = cleanGame game fallback
                model, Cmd.none, Some(StartNewGame updated)

        | Cancel -> model, Cmd.none, Some Cancelled

    let private editGameModal (game: Game) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> dispatch Cancel)
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
                                    prop.onClick (fun _ -> dispatch Cancel)
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
                                            prop.value game.Name
                                            prop.onChange (GameNameChanged >> dispatch)
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
                                            prop.value (game.Notes |> Option.defaultValue "")
                                            prop.onChange (GameNotesChanged >> dispatch)
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
                                    prop.onClick (fun _ -> dispatch Cancel)
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> dispatch Save)
                                    prop.text "Save"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private newGameModal (game: Game) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> dispatch Cancel)
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
                                    prop.onClick (fun _ -> dispatch Cancel)
                                    prop.children [ Icons.close () ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "modal-body"
                            prop.children [
                                Html.p [
                                    prop.className "form-label"
                                    prop.text
                                        "Starting a new game concludes your active session. All historical observations are preserved."
                                ]
                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "New Game Name" ]
                                        Html.input [
                                            prop.className "form-input"
                                            prop.value game.Name
                                            prop.onChange (GameNameChanged >> dispatch)
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "form-group"
                                    prop.children [
                                        Html.label [ prop.className "form-label"; prop.text "Notes (optional)" ]
                                        Html.textarea [
                                            prop.className "form-textarea"
                                            prop.value (game.Notes |> Option.defaultValue "")
                                            prop.onChange (GameNotesChanged >> dispatch)
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
                                    prop.onClick (fun _ -> dispatch Cancel)
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> dispatch Save)
                                    prop.text "Start Game"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let view (model: Model) (dispatch: Msg -> unit) =
        match model with
        | EditGame game -> editGameModal game dispatch
        | NewGame game -> newGameModal game dispatch

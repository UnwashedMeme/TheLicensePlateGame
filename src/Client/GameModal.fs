namespace TheLicensePlateGame.Client

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Browser.Types


open TheLicensePlateGame.Shared

module GameModal =

    /// Draft-editing messages. Saving and cancelling are reported to the parent via callbacks.
    type Msg =
        | GameNameChanged of string
        | GameNotesChanged of string

    /// The in-flight draft of the game being edited or created.
    type Model =
        | EditGame of Game: Game
        | NewGame of Game: Game

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

    let private init (initial: Model) : Model * Cmd<Msg> = initial, Cmd.none

    let private update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | GameNameChanged name ->
            let nextModel =
                match model with
                | EditGame game -> EditGame { game with Name = name }
                | NewGame game -> NewGame { game with Name = name }

            nextModel, Cmd.none

        | GameNotesChanged notes ->
            let nextModel =
                match model with
                | EditGame game -> EditGame { game with Notes = Some notes }
                | NewGame game -> NewGame { game with Notes = Some notes }

            nextModel, Cmd.none

    let private editGameModal (game: Game) (dispatch: Msg -> unit) (onSave: unit -> unit) (onCancel: unit -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> onCancel ())
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
                                    prop.onClick (fun _ -> onCancel ())
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
                                    prop.onClick (fun _ -> onCancel ())
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> onSave ())
                                    prop.text "Save"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private newGameModal (game: Game) (dispatch: Msg -> unit) (onSave: unit -> unit) (onCancel: unit -> unit) =
        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> onCancel ())
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
                                    prop.onClick (fun _ -> onCancel ())
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
                                    prop.onClick (fun _ -> onCancel ())
                                    prop.text "Cancel"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> onSave ())
                                    prop.text "Start Game"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    /// Self-contained modal that owns its draft state via `React.useElmish`.
    /// Reports completion through callbacks, so the parent never sees draft keystrokes.
    [<ReactComponent>]
    let Modal (initial: Model) (onSave: Game -> unit) (onStartNew: Game -> unit) (onCancel: unit -> unit) =
        let model, dispatch = React.useElmish ((fun () -> init initial), update, [||])

        match model with
        | EditGame game ->
            let save () = onSave (cleanGame game game.Name)
            editGameModal game dispatch save onCancel
        | NewGame game ->
            let save () =
                let fallback = sprintf "Trip - %s" (DateTimeOffset.Now.ToString("MMM d, yyyy"))
                onStartNew (cleanGame game fallback)
            newGameModal game dispatch save onCancel

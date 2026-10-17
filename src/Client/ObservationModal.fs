namespace TheLicensePlateGame.Client

open System
open Feliz
open TheLicensePlateGame.Shared

module ObservationModal =

    let view
        (jurisdictionId: string)
        (observations: Observation list)
        (onDelete: Guid -> unit)
        (onClose: unit -> unit) =
        let jurisdictionOpt = Jurisdictions.tryFind jurisdictionId

        let title =
            match jurisdictionOpt with
            | Some j -> sprintf "%s (%s)" j.Name j.Code
            | None -> jurisdictionId

        Html.div [
            prop.className "modal-backdrop"
            prop.onClick (fun _ -> onClose ())
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
                                    prop.ariaLabel "Close observation details"
                                    prop.onClick (fun _ -> onClose ())
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
                                                        prop.text (
                                                            obs.Timestamp.ToLocalTime().ToString("MMM d, h:mm tt")
                                                        )
                                                    ]
                                                    Html.button [
                                                        prop.className "observation-delete-button"
                                                        prop.ariaLabel "Delete observation"
                                                        prop.onClick (fun _ -> onDelete obs.Id)
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
                                    prop.onClick (fun _ -> onClose ())
                                    prop.text "Close"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

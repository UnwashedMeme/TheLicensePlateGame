namespace TheLicensePlateGame.Client

open System
open TheLicensePlateGame.Shared



/// Types of active modals or drawers with their in-flight form data.
type ModalType =
    | HamburgerMenu
    | EditGameInfo of name: string * notes: string
    | NewGameConfirm of name: string * notes: string
    | ObservationDetails of jurisdictionId: string

/// Core Elmish application state.
type Model = {
    ActiveGame: Game
    Observations: Observation list
    Scopes: Scope list
    SearchQuery: string
    HideObserved: bool
    ActiveModal: ModalType option
}

/// Elmish messages representing all user and system events.
type Msg =
    | PlateTapped of jurisdictionId: string
    | SearchChanged of string
    | ClearSearch
    | ToggleScope of Scope
    | ToggleHideObserved
    | OpenModal of ModalType
    | CloseModal
    | EditGameNameChanged of string
    | EditGameNotesChanged of string
    | SaveGameInfo
    | NewGameNameChanged of string
    | NewGameNotesChanged of string
    | ConfirmNewGame
    | DeleteObservation of id: Guid

module Model =
    /// Number of observations recorded for a given jurisdiction in the active session.
    let countForJurisdiction (jurisdictionId: string) (model: Model) : int =
        model.Observations
        |> List.fold (fun acc o -> if o.JurisdictionId = jurisdictionId then acc + 1 else acc) 0

    /// Whether a jurisdiction has at least one recorded observation.
    let isObserved (jurisdictionId: string) (model: Model) : bool =
        model.Observations
        |> List.tryFind (fun o -> o.JurisdictionId = jurisdictionId)
        |> Option.isSome

    /// Observations for a specific jurisdiction, sorted newest first.
    let observationsForJurisdiction (jurisdictionId: string) (model: Model) : Observation list =
        model.Observations
        |> List.filter (fun o -> o.JurisdictionId = jurisdictionId)
        |> List.sortByDescending (fun o -> o.Timestamp)

    /// Jurisdictions matching the set of active scopes.
    /// When no scopes are selected, displays all jurisdictions (US + Canada).
    let jurisdictionsForScopes (scopes: Scope list) : Jurisdiction list =
        let activeTags =
            scopes |> List.filter (fun s -> s.Active) |> List.map (fun s -> s.Tag)

        if List.isEmpty activeTags then
            Jurisdictions.all
        else
            Jurisdictions.all |> List.filter (Jurisdictions.hasAnyTag activeTags)

    /// Calculate (collectedUniqueCount, totalCount) for the active scopes.
    let progressForScopes (scopes: Scope list) (model: Model) : int * int =
        let targetJurisdictions = jurisdictionsForScopes scopes
        let total = targetJurisdictions.Length

        let collected =
            targetJurisdictions
            |> List.filter (fun j -> isObserved j.Id model)
            |> List.length

        collected, total

    /// Formatted progress display string, e.g. "18 / 61", "15 / 51 US", or "4 / 10 CA".
    let progressText (scopes: Scope list) (model: Model) : string =
        let collected, total = progressForScopes scopes model
        sprintf "%d / %d" collected total

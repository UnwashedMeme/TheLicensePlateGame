namespace TheLicensePlateGame.Client

open System
open TheLicensePlateGame.Shared

/// Scope filter for jurisdictions displayed on the game board.
type Scope =
    | US
    | Canada
    | All

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
    SelectedTab: Scope
    SearchQuery: string
    HideObserved: bool
    ActiveModal: ModalType option
}

/// Elmish messages representing all user and system events.
type Msg =
    | PlateTapped of jurisdictionId: string
    | SearchChanged of string
    | ClearSearch
    | TabSelected of Scope
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
        |> List.filter (fun o -> o.JurisdictionId = jurisdictionId)
        |> List.length

    /// Whether a jurisdiction has at least one recorded observation.
    let isObserved (jurisdictionId: string) (model: Model) : bool =
        countForJurisdiction jurisdictionId model > 0

    /// Observations for a specific jurisdiction, sorted newest first.
    let observationsForJurisdiction (jurisdictionId: string) (model: Model) : Observation list =
        model.Observations
        |> List.filter (fun o -> o.JurisdictionId = jurisdictionId)
        |> List.sortByDescending (fun o -> o.Timestamp)

    /// Jurisdictions belonging to the given scope.
    let jurisdictionsForScope (scope: Scope) : Jurisdiction list =
        match scope with
        | US -> Jurisdictions.usStates
        | Canada -> Jurisdictions.canadianProvinces
        | All -> Jurisdictions.all

    /// Calculate (collectedUniqueCount, totalCount) for a given scope.
    let progressForScope (scope: Scope) (model: Model) : int * int =
        let targetJurisdictions = jurisdictionsForScope scope
        let total = targetJurisdictions.Length
        let collected =
            targetJurisdictions
            |> List.filter (fun j -> isObserved j.Id model)
            |> List.length
        collected, total

    /// Formatted progress display string, e.g. "18 / 61" or "15 / 51 US".
    let progressText (scope: Scope) (model: Model) : string =
        let collected, total = progressForScope scope model
        match scope with
        | US -> sprintf "%d / %d US" collected total
        | Canada -> sprintf "%d / %d CA" collected total
        | All -> sprintf "%d / %d" collected total

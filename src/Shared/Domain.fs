namespace TheLicensePlateGame.Shared

open System

/// Represents a geographical or jurisdictional category tag with a UI-friendly display name.
type Tag =
    // Countries
    | UnitedStates of string
    | Canada of string

    // US Census Regions
    | Northeast of string
    | Midwest of string
    | South of string
    | West of string

    // US Sub-regions & Geographic Features
    | NewEngland of string
    | MidAtlantic of string
    | GreatLakes of string
    | GreatPlains of string
    | GulfStates of string
    | PacificCoast of string
    | RockyMountains of string
    | Southwest of string
    | Contiguous of string
    | NonContiguous of string

    // Canadian Regions
    | AtlanticCanada of string
    | CentralCanada of string
    | PrairieProvinces of string
    | WestCoast of string

    /// The UI-friendly display name of this tag.
    member this.UiFriendlyName =
        match this with
        | UnitedStates s
        | Canada s
        | Northeast s
        | Midwest s
        | South s
        | West s
        | NewEngland s
        | MidAtlantic s
        | GreatLakes s
        | GreatPlains s
        | GulfStates s
        | PacificCoast s
        | RockyMountains s
        | Southwest s
        | Contiguous s
        | NonContiguous s
        | AtlanticCanada s
        | CentralCanada s
        | PrairieProvinces s
        | WestCoast s -> s

    override this.ToString() = this.UiFriendlyName

/// Represents a geographic jurisdiction whose license plates can be observed.
type Jurisdiction = {
    Id: string
    Code: string
    Name: string
    Tags: Tag list
}

/// Represents the lifecycle status of a game session.
type GameStatus =
    | InProgress
    | Completed of EndDate: DateTimeOffset

/// Represents an active or completed game session.
type Game = {
    Id: Guid
    Name: string
    Notes: string option
    StartDate: DateTimeOffset
    Status: GameStatus
} with

    /// Indicates whether the game session is currently ongoing.
    member this.IsInProgress =
        match this.Status with
        | InProgress -> true
        | Completed _ -> false

    /// Indicates whether the game session has been completed.
    member this.IsCompleted =
        match this.Status with
        | Completed _ -> true
        | InProgress -> false

    /// Returns the end date if the game has completed.
    member this.EndDate =
        match this.Status with
        | Completed endDate -> Some endDate
        | InProgress -> None

/// Represents a single recorded observation of a jurisdiction license plate within a game.
type Observation = {
    Id: Guid
    GameId: Guid
    JurisdictionId: string
    Timestamp: DateTimeOffset
    Latitude: float option
    Longitude: float option
}

/// Standard tag instances with their UI-friendly names.
module Tags =
    // Countries
    let UnitedStates = Tag.UnitedStates "US States"
    let Canada = Tag.Canada "Canadian Provinces"

    // US Census Regions
    let Northeast = Tag.Northeast "Northeast"
    let Midwest = Tag.Midwest "Midwest"
    let South = Tag.South "South"
    let West = Tag.West "West"

    // US Sub-regions & Geographic Features
    let NewEngland = Tag.NewEngland "New England"
    let MidAtlantic = Tag.MidAtlantic "Mid-Atlantic"
    let GreatLakes = Tag.GreatLakes "Great Lakes"
    let GreatPlains = Tag.GreatPlains "Great Plains"
    let GulfStates = Tag.GulfStates "Gulf States"
    let PacificCoast = Tag.PacificCoast "Pacific Coast"
    let RockyMountains = Tag.RockyMountains "Rocky Mountains"
    let Southwest = Tag.Southwest "Southwest"
    let Contiguous = Tag.Contiguous "Contiguous"
    let NonContiguous = Tag.NonContiguous "Non-Contiguous"

    // Canadian Regions
    let AtlanticCanada = Tag.AtlanticCanada "Atlantic Canada"
    let CentralCanada = Tag.CentralCanada "Central Canada"
    let PrairieProvinces = Tag.PrairieProvinces "Prairie Provinces"
    let WestCoast = Tag.WestCoast "West Coast"

module Jurisdictions =
    /// Check whether a jurisdiction contains a specific tag.
    let hasTag (tag: Tag) (j: Jurisdiction) : bool = List.contains tag j.Tags

    let hasAnyTag (tags: Tag list) (j: Jurisdiction) : bool =
        List.exists (fun tag -> hasTag tag j) tags

    /// All 61 supported jurisdictions with comprehensive regional tags.
    let all: Jurisdiction list = [
        // --- US States & DC (51) ---
        {
            Id = "US-AL"
            Code = "AL"
            Name = "Alabama"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.GulfStates; Tags.Contiguous ]
        }
        {
            Id = "US-AK"
            Code = "AK"
            Name = "Alaska"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.PacificCoast; Tags.NonContiguous ]
        }
        {
            Id = "US-AZ"
            Code = "AZ"
            Name = "Arizona"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.Southwest
                Tags.RockyMountains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-AR"
            Code = "AR"
            Name = "Arkansas"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-CA"
            Code = "CA"
            Name = "California"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.PacificCoast; Tags.Contiguous ]
        }
        {
            Id = "US-CO"
            Code = "CO"
            Name = "Colorado"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.RockyMountains; Tags.Contiguous ]
        }
        {
            Id = "US-CT"
            Code = "CT"
            Name = "Connecticut"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-DE"
            Code = "DE"
            Name = "Delaware"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.MidAtlantic; Tags.Contiguous ]
        }
        {
            Id = "US-FL"
            Code = "FL"
            Name = "Florida"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.GulfStates; Tags.Contiguous ]
        }
        {
            Id = "US-GA"
            Code = "GA"
            Name = "Georgia"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-HI"
            Code = "HI"
            Name = "Hawaii"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.PacificCoast; Tags.NonContiguous ]
        }
        {
            Id = "US-ID"
            Code = "ID"
            Name = "Idaho"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.RockyMountains
                Tags.PacificCoast
                Tags.Contiguous
            ]
        }
        {
            Id = "US-IL"
            Code = "IL"
            Name = "Illinois"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatLakes; Tags.Contiguous ]
        }
        {
            Id = "US-IN"
            Code = "IN"
            Name = "Indiana"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatLakes; Tags.Contiguous ]
        }
        {
            Id = "US-IA"
            Code = "IA"
            Name = "Iowa"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-KS"
            Code = "KS"
            Name = "Kansas"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-KY"
            Code = "KY"
            Name = "Kentucky"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-LA"
            Code = "LA"
            Name = "Louisiana"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.GulfStates; Tags.Contiguous ]
        }
        {
            Id = "US-ME"
            Code = "ME"
            Name = "Maine"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-MD"
            Code = "MD"
            Name = "Maryland"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.MidAtlantic; Tags.Contiguous ]
        }
        {
            Id = "US-MA"
            Code = "MA"
            Name = "Massachusetts"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-MI"
            Code = "MI"
            Name = "Michigan"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatLakes; Tags.Contiguous ]
        }
        {
            Id = "US-MN"
            Code = "MN"
            Name = "Minnesota"
            Tags = [
                Tags.UnitedStates
                Tags.Midwest
                Tags.GreatLakes
                Tags.GreatPlains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-MS"
            Code = "MS"
            Name = "Mississippi"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.GulfStates; Tags.Contiguous ]
        }
        {
            Id = "US-MO"
            Code = "MO"
            Name = "Missouri"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-MT"
            Code = "MT"
            Name = "Montana"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.RockyMountains
                Tags.GreatPlains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-NE"
            Code = "NE"
            Name = "Nebraska"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-NV"
            Code = "NV"
            Name = "Nevada"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.Southwest; Tags.Contiguous ]
        }
        {
            Id = "US-NH"
            Code = "NH"
            Name = "New Hampshire"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-NJ"
            Code = "NJ"
            Name = "New Jersey"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.MidAtlantic; Tags.Contiguous ]
        }
        {
            Id = "US-NM"
            Code = "NM"
            Name = "New Mexico"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.Southwest
                Tags.RockyMountains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-NY"
            Code = "NY"
            Name = "New York"
            Tags = [
                Tags.UnitedStates
                Tags.Northeast
                Tags.MidAtlantic
                Tags.GreatLakes
                Tags.Contiguous
            ]
        }
        {
            Id = "US-NC"
            Code = "NC"
            Name = "North Carolina"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-ND"
            Code = "ND"
            Name = "North Dakota"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-OH"
            Code = "OH"
            Name = "Ohio"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatLakes; Tags.Contiguous ]
        }
        {
            Id = "US-OK"
            Code = "OK"
            Name = "Oklahoma"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-OR"
            Code = "OR"
            Name = "Oregon"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.PacificCoast; Tags.Contiguous ]
        }
        {
            Id = "US-PA"
            Code = "PA"
            Name = "Pennsylvania"
            Tags = [
                Tags.UnitedStates
                Tags.Northeast
                Tags.MidAtlantic
                Tags.GreatLakes
                Tags.Contiguous
            ]
        }
        {
            Id = "US-RI"
            Code = "RI"
            Name = "Rhode Island"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-SC"
            Code = "SC"
            Name = "South Carolina"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-SD"
            Code = "SD"
            Name = "South Dakota"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatPlains; Tags.Contiguous ]
        }
        {
            Id = "US-TN"
            Code = "TN"
            Name = "Tennessee"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-TX"
            Code = "TX"
            Name = "Texas"
            Tags = [
                Tags.UnitedStates
                Tags.South
                Tags.GulfStates
                Tags.Southwest
                Tags.Contiguous
            ]
        }
        {
            Id = "US-UT"
            Code = "UT"
            Name = "Utah"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.Southwest
                Tags.RockyMountains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-VT"
            Code = "VT"
            Name = "Vermont"
            Tags = [ Tags.UnitedStates; Tags.Northeast; Tags.NewEngland; Tags.Contiguous ]
        }
        {
            Id = "US-VA"
            Code = "VA"
            Name = "Virginia"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-WA"
            Code = "WA"
            Name = "Washington"
            Tags = [ Tags.UnitedStates; Tags.West; Tags.PacificCoast; Tags.Contiguous ]
        }
        {
            Id = "US-WV"
            Code = "WV"
            Name = "West Virginia"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.Contiguous ]
        }
        {
            Id = "US-WI"
            Code = "WI"
            Name = "Wisconsin"
            Tags = [ Tags.UnitedStates; Tags.Midwest; Tags.GreatLakes; Tags.Contiguous ]
        }
        {
            Id = "US-WY"
            Code = "WY"
            Name = "Wyoming"
            Tags = [
                Tags.UnitedStates
                Tags.West
                Tags.RockyMountains
                Tags.GreatPlains
                Tags.Contiguous
            ]
        }
        {
            Id = "US-DC"
            Code = "DC"
            Name = "District of Columbia"
            Tags = [ Tags.UnitedStates; Tags.South; Tags.MidAtlantic; Tags.Contiguous ]
        }

        // --- Canadian Provinces (10) ---
        {
            Id = "CA-AB"
            Code = "AB"
            Name = "Alberta"
            Tags = [ Tags.Canada; Tags.PrairieProvinces; Tags.RockyMountains ]
        }
        {
            Id = "CA-BC"
            Code = "BC"
            Name = "British Columbia"
            Tags = [ Tags.Canada; Tags.WestCoast; Tags.RockyMountains ]
        }
        {
            Id = "CA-MB"
            Code = "MB"
            Name = "Manitoba"
            Tags = [ Tags.Canada; Tags.PrairieProvinces ]
        }
        {
            Id = "CA-NB"
            Code = "NB"
            Name = "New Brunswick"
            Tags = [ Tags.Canada; Tags.AtlanticCanada ]
        }
        {
            Id = "CA-NL"
            Code = "NL"
            Name = "Newfoundland and Labrador"
            Tags = [ Tags.Canada; Tags.AtlanticCanada ]
        }
        {
            Id = "CA-NS"
            Code = "NS"
            Name = "Nova Scotia"
            Tags = [ Tags.Canada; Tags.AtlanticCanada ]
        }
        {
            Id = "CA-ON"
            Code = "ON"
            Name = "Ontario"
            Tags = [ Tags.Canada; Tags.CentralCanada; Tags.GreatLakes ]
        }
        {
            Id = "CA-PE"
            Code = "PE"
            Name = "Prince Edward Island"
            Tags = [ Tags.Canada; Tags.AtlanticCanada ]
        }
        {
            Id = "CA-QC"
            Code = "QC"
            Name = "Quebec"
            Tags = [ Tags.Canada; Tags.CentralCanada ]
        }
        {
            Id = "CA-SK"
            Code = "SK"
            Name = "Saskatchewan"
            Tags = [ Tags.Canada; Tags.PrairieProvinces ]
        }
    ]

    /// Quick filter for jurisdictions by tag (e.g., Tags.UnitedStates, Tags.GulfStates).
    let filterByTag (tag: Tag) : Jurisdiction list = all |> List.filter (hasTag tag)

    /// The 50 US States plus District of Columbia (51 jurisdictions).
    let usStates: Jurisdiction list = filterByTag Tags.UnitedStates

    /// The 10 Canadian Provinces.
    let canadianProvinces: Jurisdiction list = filterByTag Tags.Canada

    /// Quick lookup index by jurisdiction ID (e.g., "US-CA", "CA-ON").
    let byId: Map<string, Jurisdiction> =
        all |> List.map (fun j -> j.Id, j) |> Map.ofList

    /// Quick lookup index by postal code (e.g., "CA", "ON").
    let byCode: Map<string, Jurisdiction> =
        all |> List.map (fun j -> j.Code, j) |> Map.ofList

    /// Try finding a jurisdiction by its ID or postal code.
    let tryFind (idOrCode: string) : Jurisdiction option =
        match Map.tryFind idOrCode byId with
        | Some j -> Some j
        | None -> Map.tryFind idOrCode byCode

namespace TheLicensePlateGame.Shared

open System

/// Represents a geographical or jurisdictional category tag with a UI-friendly display name.
type Tag =
    // Countries
    | UnitedStates
    | Canada

    // US Census Regions
    | Northeast
    | Midwest
    | South
    | West

    // US Sub-regions & Geographic Features
    | NewEngland
    | MidAtlantic
    | GreatLakes
    | GreatPlains
    | GulfStates
    | PacificCoast
    | RockyMountains
    | Southwest
    | Contiguous
    | NonContiguous

    // Canadian Regions
    | AtlanticCanada
    | CentralCanada
    | PrairieProvinces
    | WestCoast

    /// The UI-friendly display name of this tag.
    member this.UiFriendlyName =
        match this with
        | UnitedStates -> "US States"
        | Canada -> "Canadian Provinces"

        // US Census Regions
        | Northeast -> "Northeast"
        | Midwest -> "Midwest"
        | South -> "South"
        | West -> "West"

        // US Sub-regions & Geographic Features
        | NewEngland -> "New England"
        | MidAtlantic -> "Mid-Atlantic"
        | GreatLakes -> "Great Lakes"
        | GreatPlains -> "Great Plains"
        | GulfStates -> "Gulf States"
        | PacificCoast -> "Pacific Coast"
        | RockyMountains -> "Rocky Mountains"
        | Southwest -> "Southwest"
        | Contiguous -> "Contiguous"
        | NonContiguous -> "Non-Contiguous"

        // Canadian Regions
        | AtlanticCanada -> "Atlantic Canada"
        | CentralCanada -> "Central Canada"
        | PrairieProvinces -> "Prairie Provinces"
        | WestCoast -> "West Coast"


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
    static member New() : Game = {
        Id = Guid.NewGuid()
        Name = sprintf "Trip - %s" (DateTimeOffset.Now.ToString("MMM d, yyyy"))
        Notes = None
        StartDate = DateTimeOffset.UtcNow
        Status = InProgress
    }


/// Represents a single recorded observation of a jurisdiction license plate within a game.
type Observation = {
    Id: Guid
    GameId: Guid
    JurisdictionId: string
    Timestamp: DateTimeOffset
    Latitude: float option
    Longitude: float option
}



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
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GulfStates; Tag.Contiguous ]
        }
        {
            Id = "US-AK"
            Code = "AK"
            Name = "Alaska"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.PacificCoast; Tag.NonContiguous ]
        }
        {
            Id = "US-AZ"
            Code = "AZ"
            Name = "Arizona"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.Southwest
                Tag.RockyMountains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-AR"
            Code = "AR"
            Name = "Arkansas"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-CA"
            Code = "CA"
            Name = "California"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.PacificCoast; Tag.Contiguous ]
        }
        {
            Id = "US-CO"
            Code = "CO"
            Name = "Colorado"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.RockyMountains; Tag.Contiguous ]
        }
        {
            Id = "US-CT"
            Code = "CT"
            Name = "Connecticut"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-DE"
            Code = "DE"
            Name = "Delaware"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.MidAtlantic; Tag.Contiguous ]
        }
        {
            Id = "US-FL"
            Code = "FL"
            Name = "Florida"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GulfStates; Tag.Contiguous ]
        }
        {
            Id = "US-GA"
            Code = "GA"
            Name = "Georgia"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-HI"
            Code = "HI"
            Name = "Hawaii"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.PacificCoast; Tag.NonContiguous ]
        }
        {
            Id = "US-ID"
            Code = "ID"
            Name = "Idaho"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.RockyMountains
                Tag.PacificCoast
                Tag.Contiguous
            ]
        }
        {
            Id = "US-IL"
            Code = "IL"
            Name = "Illinois"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatLakes; Tag.Contiguous ]
        }
        {
            Id = "US-IN"
            Code = "IN"
            Name = "Indiana"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatLakes; Tag.Contiguous ]
        }
        {
            Id = "US-IA"
            Code = "IA"
            Name = "Iowa"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-KS"
            Code = "KS"
            Name = "Kansas"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-KY"
            Code = "KY"
            Name = "Kentucky"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-LA"
            Code = "LA"
            Name = "Louisiana"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GulfStates; Tag.Contiguous ]
        }
        {
            Id = "US-ME"
            Code = "ME"
            Name = "Maine"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-MD"
            Code = "MD"
            Name = "Maryland"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.MidAtlantic; Tag.Contiguous ]
        }
        {
            Id = "US-MA"
            Code = "MA"
            Name = "Massachusetts"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-MI"
            Code = "MI"
            Name = "Michigan"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatLakes; Tag.Contiguous ]
        }
        {
            Id = "US-MN"
            Code = "MN"
            Name = "Minnesota"
            Tags = [
                Tag.UnitedStates
                Tag.Midwest
                Tag.GreatLakes
                Tag.GreatPlains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-MS"
            Code = "MS"
            Name = "Mississippi"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GulfStates; Tag.Contiguous ]
        }
        {
            Id = "US-MO"
            Code = "MO"
            Name = "Missouri"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-MT"
            Code = "MT"
            Name = "Montana"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.RockyMountains
                Tag.GreatPlains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-NE"
            Code = "NE"
            Name = "Nebraska"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-NV"
            Code = "NV"
            Name = "Nevada"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.Southwest; Tag.Contiguous ]
        }
        {
            Id = "US-NH"
            Code = "NH"
            Name = "New Hampshire"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-NJ"
            Code = "NJ"
            Name = "New Jersey"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.MidAtlantic; Tag.Contiguous ]
        }
        {
            Id = "US-NM"
            Code = "NM"
            Name = "New Mexico"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.Southwest
                Tag.RockyMountains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-NY"
            Code = "NY"
            Name = "New York"
            Tags = [
                Tag.UnitedStates
                Tag.Northeast
                Tag.MidAtlantic
                Tag.GreatLakes
                Tag.Contiguous
            ]
        }
        {
            Id = "US-NC"
            Code = "NC"
            Name = "North Carolina"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-ND"
            Code = "ND"
            Name = "North Dakota"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-OH"
            Code = "OH"
            Name = "Ohio"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatLakes; Tag.Contiguous ]
        }
        {
            Id = "US-OK"
            Code = "OK"
            Name = "Oklahoma"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-OR"
            Code = "OR"
            Name = "Oregon"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.PacificCoast; Tag.Contiguous ]
        }
        {
            Id = "US-PA"
            Code = "PA"
            Name = "Pennsylvania"
            Tags = [
                Tag.UnitedStates
                Tag.Northeast
                Tag.MidAtlantic
                Tag.GreatLakes
                Tag.Contiguous
            ]
        }
        {
            Id = "US-RI"
            Code = "RI"
            Name = "Rhode Island"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-SC"
            Code = "SC"
            Name = "South Carolina"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-SD"
            Code = "SD"
            Name = "South Dakota"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatPlains; Tag.Contiguous ]
        }
        {
            Id = "US-TN"
            Code = "TN"
            Name = "Tennessee"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-TX"
            Code = "TX"
            Name = "Texas"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.GulfStates; Tag.Southwest; Tag.Contiguous ]
        }
        {
            Id = "US-UT"
            Code = "UT"
            Name = "Utah"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.Southwest
                Tag.RockyMountains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-VT"
            Code = "VT"
            Name = "Vermont"
            Tags = [ Tag.UnitedStates; Tag.Northeast; Tag.NewEngland; Tag.Contiguous ]
        }
        {
            Id = "US-VA"
            Code = "VA"
            Name = "Virginia"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-WA"
            Code = "WA"
            Name = "Washington"
            Tags = [ Tag.UnitedStates; Tag.West; Tag.PacificCoast; Tag.Contiguous ]
        }
        {
            Id = "US-WV"
            Code = "WV"
            Name = "West Virginia"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.Contiguous ]
        }
        {
            Id = "US-WI"
            Code = "WI"
            Name = "Wisconsin"
            Tags = [ Tag.UnitedStates; Tag.Midwest; Tag.GreatLakes; Tag.Contiguous ]
        }
        {
            Id = "US-WY"
            Code = "WY"
            Name = "Wyoming"
            Tags = [
                Tag.UnitedStates
                Tag.West
                Tag.RockyMountains
                Tag.GreatPlains
                Tag.Contiguous
            ]
        }
        {
            Id = "US-DC"
            Code = "DC"
            Name = "District of Columbia"
            Tags = [ Tag.UnitedStates; Tag.South; Tag.MidAtlantic; Tag.Contiguous ]
        }

        // --- Canadian Provinces (10) ---
        {
            Id = "CA-AB"
            Code = "AB"
            Name = "Alberta"
            Tags = [ Tag.Canada; Tag.PrairieProvinces; Tag.RockyMountains ]
        }
        {
            Id = "CA-BC"
            Code = "BC"
            Name = "British Columbia"
            Tags = [ Tag.Canada; Tag.WestCoast; Tag.RockyMountains ]
        }
        {
            Id = "CA-MB"
            Code = "MB"
            Name = "Manitoba"
            Tags = [ Tag.Canada; Tag.PrairieProvinces ]
        }
        {
            Id = "CA-NB"
            Code = "NB"
            Name = "New Brunswick"
            Tags = [ Tag.Canada; Tag.AtlanticCanada ]
        }
        {
            Id = "CA-NL"
            Code = "NL"
            Name = "Newfoundland and Labrador"
            Tags = [ Tag.Canada; Tag.AtlanticCanada ]
        }
        {
            Id = "CA-NS"
            Code = "NS"
            Name = "Nova Scotia"
            Tags = [ Tag.Canada; Tag.AtlanticCanada ]
        }
        {
            Id = "CA-ON"
            Code = "ON"
            Name = "Ontario"
            Tags = [ Tag.Canada; Tag.CentralCanada; Tag.GreatLakes ]
        }
        {
            Id = "CA-PE"
            Code = "PE"
            Name = "Prince Edward Island"
            Tags = [ Tag.Canada; Tag.AtlanticCanada ]
        }
        {
            Id = "CA-QC"
            Code = "QC"
            Name = "Quebec"
            Tags = [ Tag.Canada; Tag.CentralCanada ]
        }
        {
            Id = "CA-SK"
            Code = "SK"
            Name = "Saskatchewan"
            Tags = [ Tag.Canada; Tag.PrairieProvinces ]
        }
    ]

    /// Quick filter for jurisdictions by tag (e.g., Tag.UnitedStates, Tag.GulfStates).
    let filterByTag (tag: Tag) : Jurisdiction list = all |> List.filter (hasTag tag)

    /// The 50 US States plus District of Columbia (51 jurisdictions).
    let usStates: Jurisdiction list = filterByTag Tag.UnitedStates

    /// The 10 Canadian Provinces.
    let canadianProvinces: Jurisdiction list = filterByTag Tag.Canada

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

namespace TheLicensePlateGame.Client

open System
open TheLicensePlateGame.Shared

/// Scope filter for jurisdictions displayed on the game board.
type Scope = {
    Tag: string
    Count: int
    Active: bool
}

module Scope =
    let CurrentlyUsed = [| Tags.UnitedStates; Tags.Canada |]
    let FromTag (tag: string) : Scope =
        let jurisdictions = Jurisdictions.filterByTag tag
        {
            Tag = tag
            Count = jurisdictions.Length
            Active = false
        }

    let Toggle (scope: Scope) : Scope = { scope with Active = not scope.Active }

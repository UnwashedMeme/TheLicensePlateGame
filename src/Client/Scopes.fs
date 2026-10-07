namespace TheLicensePlateGame.Client

open System
open TheLicensePlateGame.Shared

/// Scope filter for jurisdictions displayed on the game board.
type Scope = { Tag: Tag; Count: int; Active: bool }

module Scope =
    let currentlyUsed = [ Tag.UnitedStates; Tag.Canada ]

    let ofTag (tag: Tag) : Scope =
        let jurisdictions = Jurisdictions.filterByTag tag
        {
            Tag = tag
            Count = jurisdictions.Length
            Active = false
        }


    let toggle (scope: Scope) : Scope = { scope with Active = not scope.Active }

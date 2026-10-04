namespace TheLicensePlateGame.Client

open System
open TheLicensePlateGame.Shared

module Search =

    /// Checks if needle characters appear in haystack in the same sequential order.
    let private isSubsequence (needle: string) (haystack: string) : bool =
        if String.IsNullOrEmpty needle then true
        elif needle.Length > haystack.Length then false
        else
            let rec loop nIdx hIdx =
                if nIdx = needle.Length then true
                elif hIdx = haystack.Length then false
                elif needle.[nIdx] = haystack.[hIdx] then loop (nIdx + 1) (hIdx + 1)
                else loop nIdx (hIdx + 1)
            loop 0 0

    /// Returns the acronym / initials of words in a name (e.g. "New York" -> "ny", "North Carolina" -> "nc").
    let private acronymOf (name: string) : string =
        name.Split([| ' '; '-'; '/' |], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun word -> word.[0].ToString().ToLowerInvariant())
        |> String.concat ""

    /// Tests whether a given search query matches the jurisdiction.
    let matchesQuery (query: string) (j: Jurisdiction) : bool =
        if String.IsNullOrWhiteSpace query then
            true
        else
            let cleanQuery = query.Trim().ToLowerInvariant()
            let code = j.Code.ToLowerInvariant()
            let name = j.Name.ToLowerInvariant()

            // 1. Exact or prefix match on 2-letter postal code (e.g. "tx", "ca", "on", "la")
            if code = cleanQuery || code.StartsWith cleanQuery then
                true
            // 2. Acronym match on name words (e.g. "ny" matches "New York", "bc" matches "British Columbia")
            elif acronymOf j.Name = cleanQuery then
                true
            // 3. Name contains query as substring (e.g. "louis" matches "Louisiana", "york" matches "New York")
            elif name.Contains cleanQuery then
                true
            // 4. Any individual word in the name starts with query
            elif j.Name.Split([| ' '; '-'; '/' |], StringSplitOptions.RemoveEmptyEntries)
                 |> Array.exists (fun word -> word.ToLowerInvariant().StartsWith cleanQuery) then
                true
            // 5. Fuzzy subsequence match for queries with 3 or more characters
            elif cleanQuery.Length >= 3 && isSubsequence cleanQuery name then
                true
            else
                false

    /// Filter a list of jurisdictions by search query.
    let filter (query: string) (jurisdictions: Jurisdiction list) : Jurisdiction list =
        if String.IsNullOrWhiteSpace query then
            jurisdictions
        else
            jurisdictions |> List.filter (matchesQuery query)

namespace TheLicensePlateGame.Client

open System
open Browser.Types
open Feliz
open TheLicensePlateGame.Shared


// =========================================================================
// SVG Icons
// =========================================================================

module Icons =
    let hamburger () =
        Svg.svg [
            svg.viewBox (0, 0, 24, 24)
            svg.fill "none"
            svg.stroke "currentColor"
            svg.strokeWidth 2
            svg.children [
                Svg.line [ svg.x1 3; svg.y1 6; svg.x2 21; svg.y2 6 ]
                Svg.line [ svg.x1 3; svg.y1 12; svg.x2 21; svg.y2 12 ]
                Svg.line [ svg.x1 3; svg.y1 18; svg.x2 21; svg.y2 18 ]
            ]
        ]

    let search () =
        Svg.svg [
            svg.className "search-icon"
            svg.viewBox (0, 0, 24, 24)
            svg.fill "none"
            svg.stroke "currentColor"
            svg.strokeWidth 2
            svg.children [
                Svg.circle [ svg.cx 11; svg.cy 11; svg.r 8 ]
                Svg.line [ svg.x1 21; svg.y1 21; svg.x2 16.65; svg.y2 16.65 ]
            ]
        ]

    let close () =
        Svg.svg [
            svg.viewBox (0, 0, 24, 24)
            svg.fill "none"
            svg.stroke "currentColor"
            svg.strokeWidth 2
            svg.children [
                Svg.line [ svg.x1 18; svg.y1 6; svg.x2 6; svg.y2 18 ]
                Svg.line [ svg.x1 6; svg.y1 6; svg.x2 18; svg.y2 18 ]
            ]
        ]

    let edit () =
        Svg.svg [
            svg.className "game-title-edit-icon"
            svg.viewBox (0, 0, 24, 24)
            svg.fill "none"
            svg.stroke "currentColor"
            svg.strokeWidth 2
            svg.children [
                Svg.path [ svg.d "M12 20h9" ]
                Svg.path [ svg.d "M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" ]
            ]
        ]

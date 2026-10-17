module TheLicensePlateGame.Client.App

open Browser.Dom
open Feliz

// Mount the React root; the Elmish loop itself runs inside View.App via React.useElmish.
let private container = document.getElementById "elmish-app"
let private root = ReactDOM.createRoot container

root.render (View.App())

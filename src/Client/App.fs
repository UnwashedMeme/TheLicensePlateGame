module TheLicensePlateGame.Client.App

open Elmish
open Elmish.React

// Mount the Elmish application to the HTML root container
Program.mkProgram State.init State.update View.root
|> Program.withReactSynchronous "elmish-app"
|> Program.run

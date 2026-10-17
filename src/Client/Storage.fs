namespace TheLicensePlateGame.Client

open System
open Elmish
open Fable.Core
open Fable.Core.JsInterop
open Browser
open Browser.Types
open Browser.IndexedDBExtensions
open Thoth.Json
open TheLicensePlateGame.Shared

module Storage =

    // Use Thoth's automatic codecs: handles records, unions, options, Guids, and DateTimeOffsets out of the box.
    let private autoExtra = Extra.empty

    let encodeGame (game: Game) : string =
        Encode.Auto.toString (0, game, extra = autoExtra)

    let decodeGame (json: string) : Result<Game, string> =
        Decode.Auto.fromString<Game>(json, extra = autoExtra)

    let encodeObservation (obs: Observation) : string =
        Encode.Auto.toString (0, obs, extra = autoExtra)

    let decodeObservation (json: string) : Result<Observation, string> =
        Decode.Auto.fromString<Observation>(json, extra = autoExtra)

    let encodeGameEvent (event: GameEvent) : string =
        Encode.Auto.toString (0, event, extra = autoExtra)

    let decodeGameEvent (json: string) : Result<GameEvent, string> =
        Decode.Auto.fromString<GameEvent>(json, extra = autoExtra)

    let encodeStampedEvent (stamped: StampedGameEvent) : string =
        Encode.Auto.toString (0, stamped, extra = autoExtra)

    let decodeStampedEvent (json: string) : Result<StampedGameEvent, string> =
        Decode.Auto.fromString<StampedGameEvent>(json, extra = autoExtra)

    // =========================================================================
    // IndexedDB Event Store
    // =========================================================================

    let private dbName = "LicensePlateGameDB"
    let private dbVersion = 1
    let private storeName = "events"

    /// Envelope record structure stored in IndexedDB.
    type StoredEventRecord = {
        id: string
        gameId: string
        timestamp: float
        json: string
    }

    /// Check if IndexedDB is supported and accessible in the current browser/environment.
    let isIndexedDBAvailable () : bool =
        try
            not (isNull (box indexedDB))
        with _ ->
            false

    /// Open or initialize the IndexedDB event database.
    let openDatabase (callback: Result<IDBDatabase, string> -> unit) : unit =
        if not (isIndexedDBAvailable ()) then
            callback (Error "IndexedDB is not supported or accessible in this environment.")
        else
            try
                let req: IDBOpenDBRequest = indexedDB.``open`` (dbName, dbVersion)

                req.onupgradeneeded <-
                    fun _ ->
                        try
                            match req.result with
                            | None -> ()
                            | Some res ->
                                let db: IDBDatabase = unbox res
                                if not (db.objectStoreNames.contains storeName) then
                                    let storeOpts =
                                        jsOptions<IDBCreateStoreOptions>(fun o ->
                                            o.keyPath <- Some(U2.Case1 "id")
                                            o.autoIncrement <- Some false)

                                    let store: IDBObjectStore = db.createObjectStore (storeName, storeOpts)

                                    let indexOpts =
                                        jsOptions<IDBCreateIndexOptions>(fun o ->
                                            o.unique <- false
                                            o.multiEntry <- false)

                                    store.createIndex ("gameId", "gameId", indexOpts) |> ignore
                                    store.createIndex ("timestamp", "timestamp", indexOpts) |> ignore
                        with ex ->
                            Browser.Dom.console.error ("IndexedDB schema upgrade failed: " + ex.Message)

                req.onsuccess <-
                    fun _ ->
                        match req.result with
                        | Some res -> callback (Ok(unbox<IDBDatabase> res))
                        | None -> callback (Error "IndexedDB open succeeded but returned no database instance.")

                req.onerror <-
                    fun _ ->
                        let errMsg =
                            match req.error with
                            | null -> "Unknown IndexedDB open error"
                            | err -> err.message

                        callback (Error errMsg)
            with ex ->
                callback (Error ex.Message)

    /// Read all events from the event store ordered chronologically by timestamp index.
    let readAllEvents (callback: Result<StampedGameEvent list, string> -> unit) : unit =
        openDatabase (function
            | Error err -> callback (Error err)
            | Ok db ->
                try
                    let mutable called = false

                    let safeCallback res =
                        if not called then
                            called <- true
                            callback res

                    let tx: IDBTransaction = db.transaction (storeName, IDBTransactionMode.Readonly)

                    let store: IDBObjectStore = tx.objectStore storeName
                    let index: IDBIndex = store.index "timestamp"
                    let req: IDBRequest = index.getAll ()

                    req.onsuccess <-
                        fun _ ->
                            try
                                match req.result with
                                | None -> safeCallback (Ok [])
                                | Some res ->
                                    let rows = unbox<obj array> res

                                    let parsed =
                                        rows
                                        |> Array.choose (fun row ->
                                            let json = string (row?json)

                                            match decodeStampedEvent json with
                                            | Ok ev -> Some ev
                                            | Error err ->
                                                Browser.Dom.console.error ($"Failed to decode event: {err}", json)
                                                None)
                                        |> Array.toList

                                    safeCallback (Ok parsed)
                            with ex ->
                                safeCallback (Error ex.Message)

                    req.onerror <-
                        fun _ ->
                            let errMsg =
                                match req.error with
                                | null -> "Unknown error reading events from IndexedDB"
                                | err -> err.message

                            safeCallback (Error errMsg)

                    tx.onerror <-
                        fun _ ->
                            let errMsg =
                                match tx.error with
                                | null -> "Transaction error while reading events from IndexedDB"
                                | err -> err.message

                            safeCallback (Error errMsg)
                with ex ->
                    callback (Error ex.Message))

    /// Write a list of stamped events atomically within a single readwrite transaction.
    let writeEvents (events: StampedGameEvent list) (callback: Result<unit, string> -> unit) : unit =
        if List.isEmpty events then
            callback (Ok())
        else
            openDatabase (function
                | Error err -> callback (Error err)
                | Ok db ->
                    try
                        let mutable called = false

                        let safeCallback res =
                            if not called then
                                called <- true
                                callback res

                        let tx: IDBTransaction = db.transaction (storeName, IDBTransactionMode.Readwrite)

                        let store: IDBObjectStore = tx.objectStore storeName

                        tx.oncomplete <- fun _ -> safeCallback (Ok())

                        tx.onerror <-
                            fun _ ->
                                let errMsg =
                                    match tx.error with
                                    | null -> "Transaction error while writing events to IndexedDB"
                                    | err -> err.message

                                safeCallback (Error errMsg)

                        tx.onabort <-
                            fun _ -> safeCallback (Error "Transaction was aborted while writing events to IndexedDB")

                        for stamped in events do
                            let envelope =
                                createObj [
                                    "id" ==> string stamped.Id
                                    "gameId" ==> string stamped.GameId
                                    "timestamp" ==> float (stamped.Timestamp.ToUnixTimeMilliseconds())
                                    "json" ==> encodeStampedEvent stamped
                                ]

                            store.put envelope |> ignore
                    with ex ->
                        callback (Error ex.Message))

    /// Elmish command to load all historical events asynchronously from IndexedDB.
    let loadAllEvents (onResult: Result<StampedGameEvent list, string> -> 'msg) : Cmd<'msg> =
        Cmd.ofEffect (fun dispatch -> readAllEvents (onResult >> dispatch))

    /// Elmish command to append new events asynchronously to IndexedDB.
    let appendEvents (events: StampedGameEvent list) (onResult: Result<unit, string> -> 'msg) : Cmd<'msg> =
        if List.isEmpty events then
            Cmd.none
        else
            Cmd.ofEffect (fun dispatch -> writeEvents events (onResult >> dispatch))

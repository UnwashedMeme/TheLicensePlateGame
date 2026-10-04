namespace TheLicensePlateGame.Shared

open System

/// Event-sourcing domain events capturing state transitions for a game session.
type GameEvent =
    | PlateObserved of Observation
    | ObservationDeleted of Guid
    | GameStarted of Game
    | GameUpdated of Game
    | GameEnded of Guid

/// Stamped event envelope ready for serialization and future server synchronization.
type StampedGameEvent = {
    Id: Guid
    GameId: Guid
    Timestamp: DateTimeOffset
    Event: GameEvent
}

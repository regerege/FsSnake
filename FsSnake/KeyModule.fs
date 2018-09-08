namespace FsSnake

open System

/// input key types
type SnakeInputType =
    /// no Action
    | None
    /// moved to the up snake
    | Up
    /// moved to the right snake
    | Right
    /// moved to the down snake
    | Down
    /// moved to the left snake
    | Left
    /// continue snake game
    | Continue
    /// exit game
    | Exit
    static member convertKeyType key =
        match key with
        | ConsoleKey.UpArrow -> SnakeInputType.Up
        | ConsoleKey.RightArrow -> SnakeInputType.Right
        | ConsoleKey.LeftArrow -> SnakeInputType.Left
        | ConsoleKey.DownArrow  -> SnakeInputType.Down
        | ConsoleKey.Y -> SnakeInputType.Continue
        | ConsoleKey.N -> SnakeInputType.Exit
        | _ -> SnakeInputType.None
    static member getInput() =
        SnakeInputType.convertKeyType
            <| Console.ReadKey(true).Key

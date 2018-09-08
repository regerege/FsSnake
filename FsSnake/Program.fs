namespace FsSnake

open System

module Program =
    let retry () =
        Console.BackgroundColor <- ConsoleColor.Black
        Console.ForegroundColor <- ConsoleColor.White
        Console.SetCursorPosition(0,SnakeConfig.Height + 3)
        Console.WriteLine("try again? [Y/N]")
        let rec _retry () =
            match SnakeInputType.getInput() with
            | SnakeInputType.Continue -> true
            | SnakeInputType.Exit -> false
            | _ -> _retry()
        _retry()

    [<EntryPoint>]
    let main argv =
        let rec loop() =
            SnakeGame.run()
            if retry() then loop()
        loop()

//        Console.ReadKey(true) |> ignore
        0 // return an integer exit code

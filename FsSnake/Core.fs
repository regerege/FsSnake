namespace FsSnake

open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks

module private Print =
    let private union (conf:Config) =
        seq [
            conf.Snake
            |> List.mapi (fun i a ->
                let t =
                    if i = 0 then SnakeType.Head
                    elif i % 2 = 0 then SnakeType.BodyEven
                    else SnakeType.BodyOdd
                a,t)
            conf.Foods |> List.mapi (fun i a -> a,SnakeType.Foods)
            conf.Special |> List.mapi (fun i a -> a,SnakeType.Special)
            conf.Brown |> List.mapi (fun i a -> a,SnakeType.Brown)
            conf.Afterimage |> List.mapi (fun i a -> a,SnakeType.None)
        ]
        |> Seq.collect id
        |> Seq.groupBy fst
        |> Seq.map (fun (a,s) ->
            let t = s |> Seq.map snd |> Seq.sortBy SnakeType.ord |> Seq.head
            a,t)
    let print (conf:Config) =
        Console.BackgroundColor <- ConsoleColor.Black
        Console.ForegroundColor <- ConsoleColor.White
        Console.SetCursorPosition(0, 0)
        Console.Write(sprintf "SCORE: %09d" conf.Score)
        Console.SetCursorPosition(20, 0)
        Console.Write(sprintf "BODY: % 3d" conf.Snake.Length)
//        Console.SetCursorPosition(35, 0)
//        Console.Write(sprintf "Input: %A" conf.Input)
        union conf
        |> Seq.iter (fun ((x,y),t) ->
            let c,s = t.GetView
            Console.ForegroundColor <- c
            Console.SetCursorPosition(x+1,y+2)
            Console.Write s)
    let private printw (s:string) = Console.Write(s)
    let private printh (s:string) =
        let l = Console.CursorLeft
        let t = Console.CursorTop
        s
        |> Seq.iteri (fun i c ->
            Console.SetCursorPosition(l,t+i)
            Console.Write(c))
    let filed () =
        let view f len = f <| String.replicate len " "
        Console.Clear()
        Console.BackgroundColor <- ConsoleColor.Yellow
        Console.ForegroundColor <- ConsoleColor.Yellow
        Console.SetCursorPosition(0,1)
        printw " 12345678901234567890123456789012345678901234567890123456789012345678901234567890 "
        Console.SetCursorPosition(0,2)
        printh "123456789ABCDEFGHIJKLMNOP"
        Console.SetCursorPosition(SnakeConfig.Width+1,2)
        printh "123456789ABCDEFGHIJKLMNOP"
        Console.SetCursorPosition(0,SnakeConfig.Height+2)
        printw " 12345678901234567890123456789012345678901234567890123456789012345678901234567890 "
        
module SnakeGame =
    let private stopwatch = new Stopwatch()

    let private createInputTask() = Async.StartAsTask (async { return System.Console.ReadKey(true).Key })
    let private getInput (conf:Config) =
        let task,key =
            if conf.InputTask.IsCompleted then
                createInputTask(), SnakeInputType.convertKeyType conf.InputTask.Result
            else conf.InputTask, conf.Input
        { conf with
            Input = key
            InputTask = task
            Afterimage = [] }

    /// うんちの生成 : Generate a poo
    let rec private createBrown n (conf:Config) =
        if 0 < n then
            let head = List.last conf.Snake
            { conf with Brown = conf.Brown@[head] }
            |> createBrown (n-1)
        else conf

    /// 食事 : eat
    let rec private getFood l n =
        if 0 < n then
            let tick = stopwatch.ElapsedTicks
            let seed = (int)((tick >>> 32) ^^^ (tick &&& 0xFFFFFFFFL))
            let rand = new Random(seed)
            let x = rand.Next(SnakeConfig.Width)
            let y = rand.Next(SnakeConfig.Height)
            getFood (l@[x,y]) (n-1)
        else l

    /// 成長
    let private grow (snake:(int * int) list) =
        snake@[List.last snake]
        
    let private moveing (conf:Config) =
        let add t1 t2 =
            let a,b = t1
            let c,d = t2
            a+c,b+d
        let vector,direction =
            match conf.Input,conf.Direction with
            | SnakeInputType.Up, SnakeInputType.Left
            | SnakeInputType.Up, SnakeInputType.Right -> (0,-1),SnakeInputType.Up
            | SnakeInputType.Right, SnakeInputType.Up
            | SnakeInputType.Right, SnakeInputType.Down -> (1,0),SnakeInputType.Right
            | SnakeInputType.Down, SnakeInputType.Left
            | SnakeInputType.Down, SnakeInputType.Right -> (0,1),SnakeInputType.Down
            | SnakeInputType.Left, SnakeInputType.Up
            | SnakeInputType.Left, SnakeInputType.Down -> (-1,0),SnakeInputType.Left
            | _ -> (conf.SpeedVector),conf.Direction
        let position = add conf.Position vector
        let snake = conf.Snake
        let last = snake.Length - 1
        { conf with
            Input = SnakeInputType.None
            Position = position
            SpeedVector = vector
            Snake = [position]@(Seq.take last snake |> Seq.toList)
            Afterimage = conf.Afterimage@[snake.[last]]
            Direction = direction }

    let private collision (conf:Config) =
        let tail = List.tail conf.Snake
        let a,b = conf.Position
        let c =
            if a < 0 || b < 0 || SnakeConfig.Width <= a || SnakeConfig.Height <= b then true
            elif List.exists ((=)conf.Position) conf.Brown then true
            elif List.exists ((=)conf.Position) tail then true
            else false
        let f = List.filter ((<>)conf.Position) conf.Foods
        let s = List.filter ((<>)conf.Position) conf.Special
        let bf = f.Length <> conf.Foods.Length
        let bs = s.Length <> conf.Special.Length
        if bs || bf then
            Console.Beep()
        let sanke =
            if bf then
                conf.Snake
                |> Seq.unfold (fun snake -> Some (snake, grow snake))
                |> Seq.item SnakeConfig.StretchBody
            else conf.Snake
        let score =
            if bf then SnakeConfig.EatPoint
            elif bs then SnakeConfig.SpecialEatPoint
            else 0
        { conf with
            Score = conf.Score + score
            Input = if c then SnakeInputType.Exit else conf.Input
            Snake = sanke
            Foods = if bf then getFood f 1 else f
            Special = s
        }

    let private init() =
        let initPositionX = SnakeConfig.Width/2
        let initPositionY = SnakeConfig.Height/2
        {
            Input = SnakeInputType.None
            InputTask = createInputTask()
            Score = 0
            Ticks = 0
            Position = initPositionX,initPositionY+1
            SpeedVector = 0,1
            Snake =
                [
                    initPositionX,initPositionY+1
                    initPositionX,initPositionY
                    initPositionX,initPositionY-1
                ]
            Foods = getFood [] SnakeConfig.InitalNuberOfFood
            Special = getFood [] SnakeConfig.InitalNuberOfSpecialFood
            Brown = []
            Afterimage = []
            Direction = SnakeInputType.Down
        }
    let run () =
        stopwatch.Start()
        let rec fps (conf:Config) =
            seq {
                // input behavior
                let conf = getInput conf
                // move every 300 msec.
                let conf,moved =
                    if stopwatch.ElapsedMilliseconds % 300L = 0L then
                        moveing conf,true
                    else conf,false
                if moved then yield conf
                let conf = if moved then { conf with Afterimage = [] } else conf
                // collison detection
                let conf = if moved then collision conf else conf
                let conf =
                    if stopwatch.ElapsedMilliseconds % SnakeConfig.BrownTiming = 0L then
                        createBrown 1 conf
                    else conf
                // add special food
                let conf =
                    if stopwatch.ElapsedMilliseconds % SnakeConfig.SpecialFoodTiming = 0L then
                        { conf with Special = getFood conf.Special 1 }
                    else conf
                // grow every 5 seconds
                let conf =
                    if stopwatch.ElapsedMilliseconds % 5000L = 0L then
                        { conf with
                            Score = conf.Score + SnakeConfig.PointsScored
                            Snake = grow conf.Snake }
                    else conf
                // draw to 60 msec.
                if stopwatch.ElapsedMilliseconds % 60L = 0L then yield conf
                yield conf
                // is not performed recursion collision
                if conf.Input = SnakeInputType.Exit then
                    yield conf
                else
                    // next sequence loop
                    yield! fps conf
            }

        let conf = init()
        Print.filed()
        fps conf
        |> Seq.iter Print.print
        stopwatch.Reset()

        Console.Beep()
        Console.Beep()
        Console.Beep()
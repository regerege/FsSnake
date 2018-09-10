namespace FsSnake

open System
open System.Threading.Tasks

type SnakeType =
    | None
    | Head
    | BodyEven
    | BodyOdd
    | Brown
    | Foods
    | Special
    member x.GetView =
        match x with
        | None -> ConsoleColor.Black, " "
        | Head -> ConsoleColor.Magenta, "@"
        | BodyEven -> ConsoleColor.White, "*"
        | BodyOdd -> ConsoleColor.White, "*"
        | Brown -> ConsoleColor.DarkRed, "&"
        | Foods -> ConsoleColor.Green, "$"
        | Special -> ConsoleColor.Cyan, "#"
    member x.Order =
        match x with
        | None -> 0
        | Head -> 1
        | BodyEven -> 2
        | BodyOdd -> 3
        | Brown -> 4
        | Foods -> 6
        | Special -> 5
    static member view (t:SnakeType) = t.GetView
    static member ord (t:SnakeType) = t.Order

type Config = {
        Input : SnakeInputType
        InputTask : Task<ConsoleKey>
        Score : int
        /// game clock
        Ticks : int
        Position : int * int
        /// increase the amount of movement speed
        SpeedVector : int * int
        Snake : (int * int) list
        Foods : (int * int) list
        Special : (int * int) list
        Brown : (int * int) list
        Afterimage : (int * int) list
        Direction : SnakeInputType
    }

module internal SnakeConfig =
    let Width = 80
    let Height = 25
    /// Add a point at regular intervals
    let PointsScored = 10               //一定時間毎のポイント
    /// Add a Special Food at regular intervals
    let SpecialFoodTiming = 60000L       //スペシャル餌の追加頻度
    /// Add a Brown at regular intervals
    let BrownTiming = 3000L             //茶色い物体の追加頻度
    /// Score when I ate a meal
    let EatPoint = 100                  //餌を食べたポイント
    /// Score when I ate a special meal
    let SpecialEatPoint = 500           //スペシャル餌を食べたポイント
    /// Degree to grow and eat a meal
    let StretchBody = 1                 //えさを食べると伸びる胴体長
    let InitalNuberOfFood = 10          //初期餌数
    let InitalNuberOfSpecialFood = 1    //スペシャル餌の初期数

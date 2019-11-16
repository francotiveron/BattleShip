module BattleShipFSharpTests

open System
open NUnit.Framework
open FsUnit
open BattleShipFSharp.SinglePlayerStateTracker

let private shipAdder g (x, y, l, s) = addBattleship x y l s g 

let private exceptionChecker initialShips message ship = 
    let shipsAdder game = List.fold shipAdder game (initialShips @ [ship])
    start >> shipsAdder >> ignore |> should (throwWithMessage message) typeof<InvalidOperationException>

let private shootsChecker initialShips = 
    let shoot game (x, y, expected) = 
        let res, game' = game |> attack x y
        res |> should equal expected
        game'
    (start(), initialShips) 
    ||> List.fold shipAdder
    |> List.fold shoot
    
[<Test>]
let ``Battleship doesn't fit in board`` () =
    let check = exceptionChecker [] "The ship doesn't fit in the board"
    [
        (-1, -1, Horizontal, 1)
        (20, 3, Horizontal, 2)
        (7, 3, Horizontal, 5)
        (3, 5, Vertical, 7)
    ] 
    |> List.iter check

[<Test>]
let ``Battleship with wrong size`` () =
    let check = exceptionChecker [] "BattleShip size must be in the range [1..10]"
    [
        (1, 1, Horizontal, 0)
        (1, 1, Vertical, Int32.MaxValue)
    ]
    |> List.iter check

[<Test>]
let ``Overlapping Battleships`` () =
    let check = exceptionChecker [(5, 5, Horizontal, 5)] "The ship overlaps another ship's space"
    [
        (6, 1, Vertical, 8)
        (0, 5, Horizontal, 6)
    ]
    |> List.iter check

[<Test>]
let ``Unfinished game`` () =
    let check = shootsChecker [(3, 2, Vertical, 5)]
    [
        (3, 3, Hit)
        (3, 8, Miss)
        (3, 3, ReHit)
        (-30, 67, Miss)
    ]
    |> check
    |> hasLost
    |> should be False

[<Test>]
let ``Finished game`` () =
    let check = shootsChecker [(4, 7, Horizontal, 4); (1, 1, Vertical, 8)]
    [
        for x in [4..7] -> (x, 7, Hit)
        for y in [1..8] -> (1, y, Hit)
    ]
    |> check
    |> hasLost
    |> should be True

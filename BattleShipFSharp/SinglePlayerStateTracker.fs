module BattleShipFSharp.SinglePlayerStateTracker

[<Literal>]
let BoardSize = 10

type CellStates = Water | Ship | Wreck
type ShipLayout = Horizontal | Vertical
type AttackResult = Miss | Hit | ReHit

type Game = private {
    Board: CellStates [,]
    ShipCellsNbr: int
    HitCellsNbr: int
}

type ShipSize = ShipSize of int
[<RequireQualifiedAccess>]
module private ShipSize = 
    let create raw = 
        if raw < 1 || raw > BoardSize then
            sprintf "BattleShip size must be in the range [1..%A]" BoardSize |> invalidOp
        else
            ShipSize raw

let start() = { Board = Array2D.init BoardSize BoardSize (fun _ _ -> Water); ShipCellsNbr = 0; HitCellsNbr = 0 }

let addBattleship leftX bottomY layout size game =
    let shipSize = ShipSize.create size
    
    let rightX = leftX + match layout with | Horizontal -> (size - 1) | _ -> 0
    let topY = bottomY + match layout with | Vertical -> (size - 1) | _ -> 0

    if ([leftX >= 0; bottomY >= 0; rightX < BoardSize; topY < BoardSize] |> List.exists not) then
        invalidOp "The ship doesn't fit in the board"

    let X, Y = 
        match layout with
        | Horizontal -> (fun i -> leftX + i), (fun _ -> bottomY)
        | Vertical -> (fun _ -> leftX), (fun i -> bottomY + i)

    let rec fitShip i changedCells =
        match shipSize with
        | ShipSize sz when i < sz ->
            let x, y = X(i), Y(i)
            match game.Board.[x, y] with
            | Water -> 
                game.Board.[x, y] <- Ship
                fitShip (i + 1) ((x, y) :: changedCells)
            | _ -> Error changedCells
        | _ -> Ok ()

    match fitShip 0 [] with
    | Ok _ -> { game with ShipCellsNbr = game.ShipCellsNbr + size }
    | Error changedCells -> 
        changedCells |> List.iter (fun (x, y) -> game.Board.[x, y] <- Water)
        invalidOp "The ship overlaps another ship's space"

let attack x y game =
    let isOutOfBoard = x < 0 || y < 0 || x >= BoardSize || y >= BoardSize
    let cell = if isOutOfBoard then Water else game.Board.[x, y]
    match cell with
    | Ship ->
        game.Board.[x, y] <- Wreck
        Hit, { game with HitCellsNbr = game.HitCellsNbr + 1 }
    | Wreck -> ReHit, game
    | Water -> Miss, game

let hasLost game = game.HitCellsNbr >= game.ShipCellsNbr

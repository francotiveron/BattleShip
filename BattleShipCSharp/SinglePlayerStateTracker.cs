using System;
using System.Collections.Generic;

namespace BattleShipCSharp
{
    public class SinglePlayerStateTracker
    {
        const int BoardSize = 10;
        public struct ShipSize
        {
            int size;
            private ShipSize(int _size) { size = _size; }
            public static implicit operator ShipSize(int size)
            {
                if (size < 1 || size > BoardSize)
                    throw new InvalidOperationException($"BattleShip size must be in the range [1..{BoardSize}]");
                return new ShipSize(size);
            }
            public static implicit operator int(ShipSize size) => size.size;
        }

        public enum CellStates { Water, Ship, Wreck }
        public enum ShipLayout { Horizontal, Vertical }
        public enum AttackResult { Miss, Hit, ReHit }

        CellStates[,] board;
        int shipCellsNbr, hitCellsNbr;

        public SinglePlayerStateTracker()
        {
            //no need to seed (default 0 = Water)
            board = new CellStates[BoardSize, BoardSize];
            shipCellsNbr = hitCellsNbr = 0;
        }

        public void AddBattleship(int leftX, int bottomY, ShipLayout layout, ShipSize size)
        {
            var rightX = leftX + (layout == ShipLayout.Horizontal ? size - 1 : 0);
            var topY = bottomY + (layout == ShipLayout.Vertical ? size - 1 : 0);

            if (leftX < 0 || bottomY < 0 || rightX >= BoardSize || topY >= BoardSize)
                throw new InvalidOperationException("The ship doesn't fit in the board");

            Func<int, int> X, Y;

            if (layout == ShipLayout.Horizontal)
            {
                X = (i) => leftX + i;
                Y = (_) => bottomY;
            }
            else
            {
                X = (_) => leftX;
                Y = (i) => bottomY + i;
            }

            var changedCells = new List<(int x, int y)>();

            try
            {
                for (int i = 0; i < size; i++)
                {
                    int x = X(i), y = Y(i);

                    if (board[x, y] != CellStates.Water)
                        throw new InvalidOperationException("The ship overlaps another ship's space");

                    board[x, y] = CellStates.Ship;
                    changedCells.Add((x, y));
                }
                shipCellsNbr += size;
            }
            catch
            {
                //rollback changes
                foreach (var (x, y) in changedCells)
                    board[x, y] = CellStates.Water;

                throw;
            }
        }

        public AttackResult Attack(int x, int y)
        {
            bool isOutOfBoard = x < 0 || y < 0 || x >= BoardSize || y >= BoardSize;
            CellStates cell = isOutOfBoard ? CellStates.Water : board[x, y];

            switch (cell)
            {
                case CellStates.Ship:
                    board[x, y] = CellStates.Wreck;
                    ++hitCellsNbr;
                    return AttackResult.Hit;

                case CellStates.Wreck:
                    return AttackResult.ReHit;

                default:
                    return AttackResult.Miss;
            }
        }

        public bool HasLost => hitCellsNbr >= shipCellsNbr;
    }
}
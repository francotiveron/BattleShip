using BattleShipCSharp;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Drawing;
using Layout = BattleShipCSharp.SinglePlayerStateTracker.ShipLayout;
using Result = BattleShipCSharp.SinglePlayerStateTracker.AttackResult;

namespace BattleShipCSharpTests
{
    public class Tests
    {
        SinglePlayerStateTracker tracker;

        #region Utils
        void CheckExcept(int x, int y, Layout layout, int size, string message) =>
            Assert.Throws(Is.TypeOf<InvalidOperationException>()
            .And.Message.EqualTo(message),
            () => tracker.AddBattleship(x, y, layout, size));

        Action<int, int, Layout, int> ExceptionChecker(string message)
        {
            return (x, y, l, s) => CheckExcept(x, y, l, s, message);
        }

        void CheckShoot(int x, int y, Result r) => Assert.AreEqual(r, tracker.Attack(x, y));

        void AddBattleShip(int x, int y, Layout l, int s) => tracker.AddBattleship(x, y, l, s);

        void CheckHasLost() => Assert.True(tracker.HasLost);

        void CheckHasNotLost() => Assert.False(tracker.HasLost);
        #endregion

        [SetUp]
        public void Setup()
        {
            tracker = new SinglePlayerStateTracker();
        }

        [Test]
        public void NonFittingBattleship()
        {
            var Check = ExceptionChecker("The ship doesn't fit in the board");

            Check(-1, -1, Layout.Horizontal, 1);
            Check(20, 3, Layout.Horizontal, 2);
            Check(7, 3, Layout.Horizontal, 5);
            Check(3, 5, Layout.Vertical, 7);
        }

        [Test]
        public void WronglySizedBattleship()
        {
            var Check = ExceptionChecker("BattleShip size must be in the range [1..10]");

            Check(1, 1, Layout.Horizontal, 0);
            Check(1, 1, Layout.Vertical, int.MaxValue);
        }

        [Test]
        public void OverlappingBattleships()
        {
            var Check = ExceptionChecker("The ship overlaps another ship's space");

            AddBattleShip(5, 5, Layout.Horizontal, 5);
            Check(6, 1, Layout.Vertical, 8);
            Check(0, 5, Layout.Horizontal, 6);
        }

        [Test]
        public void UnfinishedGame()
        {
            AddBattleShip(3, 2, Layout.Vertical, 5);
            CheckShoot(3, 3, Result.Hit);
            CheckShoot(3, 8, Result.Miss);
            CheckShoot(3, 3, Result.ReHit);
            CheckShoot(-30, 67, Result.Miss);
            CheckHasNotLost();
        }

        [Test]
        public void FinishedGame()
        {
            AddBattleShip(4, 7, Layout.Horizontal, 4);
            AddBattleShip(1, 1, Layout.Vertical, 8);
            for (int x = 4; x <= 7; x++) CheckShoot(x, 7, Result.Hit);
            for (int y = 1; y <= 8; y++) CheckShoot(1, y, Result.Hit);
            CheckHasLost();
        }
    }
}
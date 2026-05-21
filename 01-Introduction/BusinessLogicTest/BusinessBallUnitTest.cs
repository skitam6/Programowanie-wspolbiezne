//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            List<Ball> dummyList = new List<Ball>();
            object dummyLock = new object();

            Ball newInstance = new(dataBallFixture, BusinessLogicAbstractAPI.GetDimensions, dummyList, dummyLock);

            int numberOfCallBackCalled = 0;
            newInstance.PositionChanged += (sender, e) => { Assert.IsNotNull(sender); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public double Mass { get; } = 10.0;
            public double Radius { get; } = 10.0;
            public Data.IVector Position { get; } = new VectorFixture(0.0, 0.0);
            public Data.IVector Velocity { get; set; } = new VectorFixture(1.0, 1.0);

            public event EventHandler? PositionChanged;

            internal void Move()
            {
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Dispose() { }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}
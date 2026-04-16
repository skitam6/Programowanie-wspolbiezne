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
using TP.ConcurrentProgramming.Data;


namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {

        private readonly TP.ConcurrentProgramming.Data.IBall _dataBall;
        private readonly Dimensions _dimensions;

        public Ball(TP.ConcurrentProgramming.Data.IBall ball, Dimensions dimensions)
        {
            _dataBall = ball;
            _dimensions = dimensions;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            double diameter = _dimensions.BallDimension;

            double velX = _dataBall.Velocity.x;
            double velY = _dataBall.Velocity.y;
            bool bounced = false;

            if (e.x <= 0 || e.x + diameter >= _dimensions.TableWidth)
            {
                velX = -velX; 
                bounced = true;
            }

            if (e.y <= 0 || e.y + diameter >= _dimensions.TableHeight)
            {
                velY = -velY;
                bounced = true;
            }
            if (bounced)
            {
                _dataBall.Velocity = new Vector(velX, velY);
            }

            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private

        internal record Vector(double x, double y) : Data.IVector;
    }
}
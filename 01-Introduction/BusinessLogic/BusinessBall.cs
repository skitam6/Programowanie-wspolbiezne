using System;
using TP.ConcurrentProgramming.Data;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly TP.ConcurrentProgramming.Data.IBall _dataBall;
        private readonly Dimensions _dimensions;
        private readonly List<Ball> _allBalls;
        private readonly object _collisionLock;

        public Ball(TP.ConcurrentProgramming.Data.IBall ball, Dimensions dimensions, List<Ball> allBalls, object collisionLock)
        {
            _dataBall = ball;
            _dimensions = dimensions;
            _allBalls = allBalls;
            _collisionLock = collisionLock;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            lock (_collisionLock)
            {
                double diameter = _dimensions.BallDimension;
                double radius = diameter / 2;

                double velX = _dataBall.Velocity.x;
                double velY = _dataBall.Velocity.y;
                bool velocityChanged = false;

                if ((e.x <= 0 && velX < 0) || (e.x + diameter >= _dimensions.TableWidth && velX > 0))
                {
                    velX = -velX;
                    velocityChanged = true;
                }


                if ((e.y <= 0 && velY < 0) || (e.y + diameter >= _dimensions.TableHeight && velY > 0))
                {
                    velY = -velY;
                    velocityChanged = true;
                }

                foreach (var otherBall in _allBalls)
                {
                    if (otherBall == this) continue;

                    double dx = (e.x + radius) - (otherBall._dataBall.Position.x + radius);
                    double dy = (e.y + radius) - (otherBall._dataBall.Position.y + radius);
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= diameter)
                    {
                        double otherVelX = otherBall._dataBall.Velocity.x;
                        double otherVelY = otherBall._dataBall.Velocity.y;

                        double relVelX = velX - otherVelX;
                        double relVelY = velY - otherVelY;

                        if ((relVelX * dx + relVelY * dy) < 0)
                        {

                            double nx = dx / distance;
                            double ny = dy / distance;
                            double tx = -ny;
                            double ty = nx;

                            double dpNorm1 = velX * nx + velY * ny;
                            double dpTan1 = velX * tx + velY * ty;

                            double dpNorm2 = otherVelX * nx + otherVelY * ny;
                            double dpTan2 = otherVelX * tx + otherVelY * ty;

                            double dpNorm1_new = dpNorm2;
                            double dpNorm2_new = dpNorm1;

                            velX = (dpNorm1_new * nx) + (dpTan1 * tx);
                            velY = (dpNorm1_new * ny) + (dpTan1 * ty);
                            velocityChanged = true;

                            double newOtherVelX = (dpNorm2_new * nx) + (dpTan2 * tx);
                            double newOtherVelY = (dpNorm2_new * ny) + (dpTan2 * ty);

                            otherBall._dataBall.Velocity = new Vector(newOtherVelX, newOtherVelY);

                        }
                    }
                }

                if (velocityChanged)
                {
                    _dataBall.Velocity = new Vector(velX, velY);
                }
            }

            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private

        internal record Vector(double x, double y) : Data.IVector;
    }
}
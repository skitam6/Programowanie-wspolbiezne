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

            _dataBall.PositionChanged += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler? PositionChanged;

        #endregion IBall
        public double Mass => _dataBall.Mass;
        public double Radius => _dataBall.Radius;
        public IPosition Position => new Position(_dataBall.Position.x, _dataBall.Position.y);

        #region private

        private void RaisePositionChangeEvent(object? sender, EventArgs e)
        {
            lock (_collisionLock)
            {
                double diameter = _dataBall.Radius * 2;
                double radius = _dataBall.Radius;

                var currentPos = _dataBall.Position;
                var currentVel = _dataBall.Velocity;

                double velX = currentVel.x;
                double velY = currentVel.y;
                bool velocityChanged = false;

                if ((currentPos.x <= 0 && velX < 0) || (currentPos.x + diameter >= _dimensions.TableWidth && velX > 0))
                {
                    velX = -velX;
                    velocityChanged = true;
                }
                if ((currentPos.y <= 0 && velY < 0) || (currentPos.y + diameter >= _dimensions.TableHeight && velY > 0))
                {
                    velY = -velY;
                    velocityChanged = true;
                }

                foreach (var otherBall in _allBalls)
                {
                    if (otherBall == this) continue;

                    var otherPos = otherBall._dataBall.Position;
                    double dx = (currentPos.x + radius) - (otherPos.x + otherBall.Radius);
                    double dy = (currentPos.y + radius) - (otherPos.y + otherBall.Radius);
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= (radius + otherBall.Radius))
                    {
                        var otherVel = otherBall._dataBall.Velocity;
                        double relVelX = velX - otherVel.x;
                        double relVelY = velY - otherVel.y;

                        if ((relVelX * dx + relVelY * dy) < 0)
                        {
                            double nx = dx / distance;
                            double ny = dy / distance;

                            double v1n = velX * nx + velY * ny;
                            double v2n = otherVel.x * nx + otherVel.y * ny;
                            double v1t = velX * -ny + velY * nx;
                            double v2t = otherVel.x * -ny + otherVel.y * nx;

                            double v1n_new = v2n;
                            double v2n_new = v1n;

                            velX = v1n_new * nx + v1t * -ny;
                            velY = v1n_new * ny + v1t * nx;
                            velocityChanged = true;

                            double newOtherVelX = v2n_new * nx + v2t * -ny;
                            double newOtherVelY = v2n_new * ny + v2t * nx;

                            otherBall._dataBall.Velocity = new Vector(newOtherVelX, newOtherVelY);
                        }
                    }
                }

                if (velocityChanged)
                {
                    _dataBall.Velocity = new Vector(velX, velY);
                }
            }

            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion private

        internal record Vector(double x, double y) : Data.IVector;
    }
}
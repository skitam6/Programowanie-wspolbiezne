using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor
        private IVector _position;
        private IVector _velocity;
        private readonly object _lock = new object();
        private readonly Action<double, double, double, double> _logAction;

        public IVector Position
        {
            get { lock (_lock) return _position; }
            private set { lock (_lock) _position = value; }
        }

        public IVector Velocity
        {
            get { lock (_lock) return _velocity; }
            set { lock (_lock) _velocity = value; }
        }

        public event EventHandler? PositionChanged;
        private CancellationTokenSource _cancellationTokenSource;

        internal Ball(Vector initialPosition, Vector initialVelocity, Action<double, double, double, double> logAction)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            _logAction = logAction;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => MoveLoop(_cancellationTokenSource.Token));
        }
        private async Task MoveLoop(CancellationToken token)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!token.IsCancellationRequested)
            {
                int currentDelay;

                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();

                double timeMultiplier = elapsedMilliseconds > 0 ? (double)elapsedMilliseconds / 16.0 : 1.0;

                lock (_lock)
                {
                    _position = new Vector(
                        _position.x + (_velocity.x * timeMultiplier),
                        _position.y + (_velocity.y * timeMultiplier)
                    );

                    double speed = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);

                    if (speed > 0.1)
                    {
                        currentDelay = (int)Math.Clamp(50.0 / speed, 10.0, 24.0);
                    }
                    else
                    {
                        currentDelay = 24;
                    }
                }

                PositionChanged?.Invoke(this, EventArgs.Empty);

                _logAction?.Invoke(Position.x, Position.y, Velocity.x, Velocity.y);

                await Task.Delay(currentDelay, token);
            }
        }
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        #endregion ctor

        #region IBall


    #endregion IBall

    #region private
        #endregion private
    }
}
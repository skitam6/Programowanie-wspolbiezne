using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor
        public double Mass { get; init; }
        public double Radius { get; init; }

        private IVector _position;
        private IVector _velocity;
        private readonly object _lock = new object();

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

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => MoveLoop(_cancellationTokenSource.Token));
        }
        private async Task MoveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int currentDelay;

                lock (_lock)
                {
                    _position = new Vector(_position.x + _velocity.x, _position.y + _velocity.y);

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
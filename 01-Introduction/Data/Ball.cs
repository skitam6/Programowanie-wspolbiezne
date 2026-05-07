using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor
        public double Mass { get; init; }
        public double Radius { get; init; }
        public IVector Position {  get; private set; }
        public IVector Velocity { get; set; }

        public event EventHandler<IVector>? NewPositionNotification;
        private CancellationTokenSource _cancellationTokenSource;

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Mass = mass;
            Radius = radius;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => MoveLoop(_cancellationTokenSource.Token));
        }
        private async Task MoveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
                NewPositionNotification?.Invoke(this, Position);

                // Opóźnienie pętli (~60 FPS)
                await Task.Delay(16, token);
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

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }


        #endregion private
    }
}
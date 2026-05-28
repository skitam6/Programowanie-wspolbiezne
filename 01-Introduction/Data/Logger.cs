using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal record DiagnosticData(DateTime Timestamp, string Message, double X, double Y, double VelX, double VelY);

    internal class Logger : IDisposable
    {
        private readonly ConcurrentQueue<DiagnosticData> _buffer = new ConcurrentQueue<DiagnosticData>();
        private readonly int _maxBufferSize = 100;
        private readonly string _filePath = "diagnostic_log.json";
        private readonly Task _loggingTask;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _bufferFullLogged = false;

        public Logger()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            _loggingTask = Task.Run(WriteLoop);
        }

        public void LogBallState(double x, double y, double velX, double velY)
        {
            if (_buffer.Count >= _maxBufferSize)
            {
                if (!_bufferFullLogged)
                {
                    _buffer.Enqueue(new DiagnosticData(DateTime.Now, "BUFFER_OVERFLOW_DATA_LOST", 0, 0, 0, 0));
                    _bufferFullLogged = true;
                }
                return;
            }

            _bufferFullLogged = false;

            _buffer.Enqueue(new DiagnosticData(DateTime.Now, "BallState", x, y, velX, velY));
        }

        private async Task WriteLoop()
        {
            using StreamWriter sw = new StreamWriter(_filePath, append: true, encoding: System.Text.Encoding.ASCII);

            var options = new JsonSerializerOptions { WriteIndented = true };

            while (!_cts.Token.IsCancellationRequested)
            {
                if (_buffer.TryDequeue(out DiagnosticData data))
                {
                    string jsonString = JsonSerializer.Serialize(data, options);
                    await sw.WriteLineAsync(jsonString);
                    await sw.WriteLineAsync();
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _loggingTask.Wait();
        }
    }
}
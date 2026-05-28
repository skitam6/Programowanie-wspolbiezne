using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    // Rekord reprezentujący STAN OBIEKTU (Snapshot danych do logu)
    internal record DiagnosticData(DateTime Timestamp, string Message, double X, double Y, double VelX, double VelY);

    internal class Logger : IDisposable
    {
        private readonly ConcurrentQueue<DiagnosticData> _buffer = new ConcurrentQueue<DiagnosticData>();
        private readonly int _maxBufferSize = 100; // Limit bufora (celowo mały, by obsłużyć przepełnienie)
        private readonly string _filePath = "diagnostic_log.json";
        private readonly Task _loggingTask;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _bufferFullLogged = false;

        public Logger()
        {
            // Czyścimy plik przy każdym nowym starcie symulacji
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            // Odpalamy wątek konsumenta w tle
            _loggingTask = Task.Run(WriteLoop);
        }

        // Ta metoda jest wywoływana przez różne Kule (wiele wątków)
        public void LogBallState(double x, double y, double velX, double velY)
        {
            // 1. Sprawdzamy stan bufora (Przypadek: Bufor jest pełny)
            if (_buffer.Count >= _maxBufferSize)
            {
                if (!_bufferFullLogged)
                {
                    // Wyrzucamy dane do śmieci, ale zapisujemy FAKT przepełnienia i utraty danych
                    _buffer.Enqueue(new DiagnosticData(DateTime.Now, "BUFFER_OVERFLOW_DATA_LOST", 0, 0, 0, 0));
                    _bufferFullLogged = true;
                }
                return; // Wyrzucamy dane kuli do śmieci (nie blokujemy jej wątku!)
            }

            _bufferFullLogged = false;

            // Tworzymy stempel czasowy (Time-stamp) i zapisujemy stan bez serializacji!
            _buffer.Enqueue(new DiagnosticData(DateTime.Now, "BallState", x, y, velX, velY));
        }

        // To jest nasz JEDEN dedykowany wątek do zapisu i konwersji
        private async Task WriteLoop()
        {
            using StreamWriter sw = new StreamWriter(_filePath, append: true);

            while (!_cts.Token.IsCancellationRequested)
            {
                // Jeśli w buforze coś jest (Przypadek: Bufor ma dane)
                if (_buffer.TryDequeue(out DiagnosticData data))
                {
                    // SERIALIZACJA - odbywa się na dedykowanym wątku dyskowym, 
                    // zgodnie z wymogiem prowadzącego, aby nie opóźniać wątków kul!
                    string jsonString = JsonSerializer.Serialize(data);
                    await sw.WriteLineAsync(jsonString);
                }
                else
                {
                    // Przypadek: Bufor jest pusty. Wątek logujący idzie spać.
                    await Task.Delay(10);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _loggingTask.Wait(); // Czekamy aż wątek zapisu zakończy pracę
        }
    }
}
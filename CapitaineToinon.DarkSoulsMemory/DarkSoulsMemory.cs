using System;
using System.Diagnostics;
using System.Threading;

namespace CapitaineToinon.DarkSoulsMemory
{
    public class DarkSoulsMemory
    {
        public event EventHandler OnGameProgressUpdated;

        private DarkSoulsProcess darksouls;
        private Stopwatch stopwatch;
        private Thread mainThread;
        private CancellationTokenSource tokenSource;

        public bool IsRunning
        {
            get
            {
                return (mainThread != null && mainThread.IsAlive);
            }
        }

        public DarkSoulsMemory()
        {
            this.darksouls = new DarkSoulsProcess();
        }

        ~DarkSoulsMemory()
        {
            this.darksouls.OnGameProgressUpdated -= Darksouls_OnGameProgressUpdated;
        }

        private void Darksouls_OnGameProgressUpdated(object sender, EventArgs e)
        {
            OnGameProgressUpdated?.Invoke(sender, e);
        }

        private void MainThreadFunction(ref CancellationTokenSource source)
        {
            long lastCall = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!source.Token.IsCancellationRequested)
            {
                if (stopwatch.ElapsedMilliseconds - lastCall > Constants.MAIN_THREAD_REFRESH_MS)
                {
                    darksouls.Next();
                    lastCall = stopwatch.ElapsedMilliseconds;
                }
            }

            // Thread got canceled
        }

        public void Start()
        {
            this.darksouls.OnGameProgressUpdated += Darksouls_OnGameProgressUpdated;
            tokenSource = new CancellationTokenSource();
            if (mainThread == null || !mainThread.IsAlive)
            {
                mainThread = new Thread(() => MainThreadFunction(ref tokenSource))
                {
                    IsBackground = true
                };
                mainThread.Start();
            }
        }

        public void Stop()
        {
            this.darksouls.OnGameProgressUpdated -= Darksouls_OnGameProgressUpdated;
            if (tokenSource != null && !tokenSource.Token.IsCancellationRequested && mainThread.IsAlive)
            {
                tokenSource.Cancel();
            }
        }

        public void Quit()
        {
            if (mainThread != null)
                mainThread.Abort();
            darksouls.Quit();
        }
    }
}

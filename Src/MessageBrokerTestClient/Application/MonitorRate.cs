using Khooversoft.Toolbox;
using System;
using System.Threading;

namespace MessageBrokerTestClient
{
    internal class MonitorRate : IDisposable
    {
        private static TimeSpan _period = TimeSpan.FromSeconds(1);
        private RateDetail _currentRateDetail;
        private Timer _timer;
        private int _flushLock = 0;

        public MonitorRate(MonitorReport report, string name)
        {
            Verify.IsNotNull(nameof(report), report);
            Verify.IsNotEmpty(nameof(name), name);

            Report = report;
            Name = name;

            _timer = new Timer(x => Flush(), null, _period, _period);
            _currentRateDetail = new RateDetail(name);
        }

        public MonitorReport Report { get; }

        public string Name { get; }

        public void IncrementNew(int value = 1)
        {
            Verify.Assert(_timer != null, "Monitor is not running");

            _currentRateDetail.AddNew(value);
        }

        public void IncrementRead(int value = 1)
        {
            Verify.Assert(_timer != null, "Monitor is not running");

            _currentRateDetail.AddRead(value);
        }

        public void IncrementError(int value = 1)
        {
            Verify.Assert(_timer != null, "Monitor is not running");

            _currentRateDetail.AddError(value);
        }

        public void IncrementError(string errorMessage)
        {
            Verify.Assert(_timer != null, "Monitor is not running");

            _currentRateDetail.AddError(errorMessage);
        }

        public void AddRetry(int value = 1)
        {
            Verify.Assert(_timer != null, "Monitor is not running");

            _currentRateDetail.AddRetryCount(value);
        }

        public void Dispose()
        {
            Timer timer = Interlocked.Exchange(ref _timer, null);
            timer?.Dispose();

            if (timer != null)
            {
                Flush();
            }
        }

        private void Flush()
        {
            // Only allow one thread in (non blocking)
            int currentLock = Interlocked.CompareExchange(ref _flushLock, 1, 0);
            if (currentLock == 1)
            {
                return;
            }

            RateDetail current = Interlocked.Exchange(ref _currentRateDetail, new RateDetail(Name));
            current.Stop();
            Report.Enqueue(current);

            Interlocked.Exchange(ref _flushLock, 0);
            return;
        }
    }
}

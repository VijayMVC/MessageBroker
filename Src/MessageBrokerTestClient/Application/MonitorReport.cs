// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MessageBrokerTestClient
{
    internal class MonitorReport : IDisposable
    {
        private readonly IOptions _option;
        private readonly ConcurrentQueue<RateDetail> _queue = new ConcurrentQueue<RateDetail>();
        private Timer _timer;
        private int _lock = 0;

        public MonitorReport(string name, IOptions option)
        {
            Verify.IsNotEmpty(nameof(name), name);
            Verify.IsNotNull(nameof(option), option);

            Name = name;
            _option = option;
            _timer = new Timer(x => DumpQueue(), null, _option.SampleRate, _option.SampleRate);
        }

        public string Name { get; }

        public void Enqueue(RateDetail value)
        {
            Verify.Assert(_timer != null, "Report monitor is not running");

            _queue.Enqueue(value);
        }

        public void Dispose()
        {
            Timer timer = Interlocked.Exchange(ref _timer, null);
            timer?.Dispose();

            if (timer != null)
            {
                DumpQueue();
            }
        }

        private void DumpQueue()
        {
            // Only allow one thread in (non blocking)
            int currentLock = Interlocked.CompareExchange(ref _lock, 1, 0);
            if (currentLock == 1)
            {
                return;
            }

            if (_option.ShowDetail)
            {
                DisplayDetail();
            }
            else
            {
                DisplaySummary();
            }

            Interlocked.Exchange(ref _lock, 0);
            return;
        }

        private void DisplaySummary()
        {
            var list = new List<RateDetail>();
            while (_queue.Count > 0)
            {
                RateDetail detail;
                if (_queue.TryDequeue(out detail))
                {
                    detail.Stop();
                    list.Add(detail);
                }
            }

            RateDetail summary = new RateDetail($"Summary ({list.Count})");
            foreach (var item in list)
            {
                summary.Add(item);
            }

            summary.Stop();
            DisplayDetail(summary);
        }

        private void DisplayDetail()
        {
            while (_queue.Count > 0)
            {
                RateDetail detail;
                if (_queue.TryDequeue(out detail))
                {
                    detail.Stop();
                    DisplayDetail(detail);
                }
            }
        }

        private void DisplayDetail(RateDetail detail)
        {
            int count = detail.NewCount + detail.ReadCount;

            var fields = new List<string>
            {
                $"{Name,-10}",
                $"Service={detail.Name,-20}",
                $"Count=({count,5:D} / {detail.TpsRate:00000.00})",
                $"Read=({detail.ReadCount, 5:D} / {detail.TpsReadRate:00000.00})",
                $"New=({detail.NewCount, 5:D} / {detail.TpsNewRate:00000.00})",
                $"Error Count={detail.ErrorCount, 5:D}",
                $"Retry={detail.RetryCount, 5:D}",
            };

            if (detail.LastErrorMessage.IsNotEmpty())
            {
                fields.Add($"LastError: {detail.LastErrorMessage}");
            }

            Console.WriteLine(string.Join(", ", fields));
        }
    }
}

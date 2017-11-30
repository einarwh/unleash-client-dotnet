﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unleash.Scheduling;

namespace Unleash.Tests.Scheduling
{
    public class TimerTaskRunnerTests
    {
        class NoopBackgroundTask : IUnleashScheduledTask
        {
            private readonly ManualResetEvent reset;
            private readonly TimeSpan delay;

            public NoopBackgroundTask(ManualResetEvent reset, TimeSpan delay)
            {
                this.reset = reset;
                this.delay = delay;
            }

            public int Counter { get; set; } = 1;

            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Counter++;
                Console.WriteLine($"{Counter} {DateTime.Now.Millisecond}");
                await Task.Delay(delay, cancellationToken);

                if (Counter == 10)
                {
                    reset.Set();
                }
            }

            public string Name => "noop-task";
            public TimeSpan Interval { get; set; }
            public bool ExecuteDuringStartup { get; set; }
        }

        class FailureBackgroundTask : IUnleashScheduledTask
        {
            public int Counter { get; set; }

            private readonly ManualResetEvent reset;

            public FailureBackgroundTask(ManualResetEvent reset)
            {
                this.reset = reset;
            }

            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}");
                Counter++;

                if (Counter == 10)
                {
                    reset.Set();
                    return;
                }

                await Task.Delay(10, cancellationToken);

                throw new Exception("Wops");
            }

            public string Name => "Demo";
            public TimeSpan Interval { get; set; }
            public bool ExecuteDuringStartup { get; set; }
        }

        //[Test]
        //public void HappyPath()
        //{
        //    using (var reset = new ManualResetEvent(false))
        //    {
        //        var task = new NoopBackgroundTask(reset, TimeSpan.FromMilliseconds(100));

        //        using (new TimerTaskRunner(task, TimeSpan.FromMilliseconds(1), true, new CancellationToken()))
        //        {
        //            reset.WaitOne(1200);
        //        }

        //        task.Counter.Should().Be(10);
        //    }
        //}

        //[Test]
        //public void Exceptions_Should_Not_Stop_Execution()
        //{
        //    using (var reset = new ManualResetEvent(false))
        //    {
        //        var task = new FailureBackgroundTask(reset);
        //        using (new TimerTaskRunner(task, TimeSpan.FromMilliseconds(1), true, new CancellationToken()))
        //        {
        //            reset.WaitOne(1000);
        //        }

        //        task.Counter.Should().Be(10);
        //    }
        //}

        [Test]
        public void Cancellation_ShouldStopFutherProcessing()
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);

                //using (var reset = new ManualResetEvent(false))
                //{
                //    var task = new NoopBackgroundTask(reset, TimeSpan.FromMilliseconds(5));

                //    using (new TimerTaskRunner(task, TimeSpan.FromMilliseconds(1), true, cts.Token))
                //    {
                //        reset.WaitOne(200);
                //    }

                //    task.Counter.Should().BeLessThan(10);
                //}
            }
        }
    }
}
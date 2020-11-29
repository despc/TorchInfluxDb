﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Utils.General
{
    internal static class TaskUtils
    {
        public static async void Forget(this Task self, ILogger logger)
        {
            await self.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.Error(t.Exception);
                }
            });
        }

        public static Task MoveToThreadPool(CancellationToken canceller = default)
        {
            canceller.ThrowIfCancellationRequested();

            var taskSource = new TaskCompletionSource<byte>();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                canceller.ThrowIfCancellationRequested();
                taskSource.SetResult(0);
            });

            return taskSource.Task;
        }

        public static Task RunUntilCancelledAsync(this CancellationToken self, Func<CancellationToken, Task> f)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    await f(self);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            }, self);
        }
    }
}
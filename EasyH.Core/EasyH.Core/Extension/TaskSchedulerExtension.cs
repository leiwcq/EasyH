using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyH.Core.Extension
{
    public static class TaskSchedulerExtension
    {
        public static async Task<T> StartNew<T>(this TaskScheduler taskScheduler, Func<T> func)
        {
            var task = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            return await task;
        }

        public static async Task StartNew(this TaskScheduler taskScheduler, Action func)
        {
            var task = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            await task;
        }

        public static async Task<T> StartNew<T>(this Func<T> func, TaskScheduler taskScheduler)
        {
            var task = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            return await task;
        }

        public static async Task StartNew(this Action func, TaskScheduler taskScheduler)
        {
            var task = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            await task;
        }
    }
}
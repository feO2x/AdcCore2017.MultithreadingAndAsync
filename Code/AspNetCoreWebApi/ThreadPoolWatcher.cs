using System;
using System.Threading;

namespace AspNetCoreWebApi
{
    public class ThreadPoolWatcher
    {
        public static readonly ThreadPoolWatcher Instance = new ThreadPoolWatcher();
        public readonly int MaximumCompletionPortThreads;
        public readonly int MaximumWorkerThreads;
        private int _usedCompletionPortThreads;
        private int _usedWorkerThreads;

        public ThreadPoolWatcher()
        {
            ThreadPool.GetMaxThreads(out MaximumWorkerThreads, out MaximumCompletionPortThreads);
        }

        public int UsedWorkerThreads => Volatile.Read(ref _usedWorkerThreads);
        public int UsedCompletionPortThreads => Volatile.Read(ref _usedCompletionPortThreads);

        public void UpdateUsedThreads()
        {
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);

            InterlockedMaximum(ref _usedWorkerThreads, MaximumWorkerThreads - availableWorkerThreads);
            InterlockedMaximum(ref _usedCompletionPortThreads, MaximumCompletionPortThreads - availableCompletionPortThreads);
        }

        private static void InterlockedMaximum(ref int target, int value)
        {
            int temporaryValue;
            var readValueOfTarget = target;
            do
            {
                temporaryValue = readValueOfTarget;
                readValueOfTarget = Interlocked.CompareExchange(ref target, Math.Max(temporaryValue, value), temporaryValue);
            } while (temporaryValue != readValueOfTarget);
        }
    }
}
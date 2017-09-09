/* This is a modified sample of Jeffrey Richter's Advanced .NET Threading Course, available at:
 * https://mva.microsoft.com/en-US/training-courses/advanced-net-threading-part-1-thread-fundamentals-16656
 */
using System;
using System.Diagnostics;
using System.Threading;

namespace ThreadSize
{
    public static class Program
    {
        private static readonly ManualResetEventSlim Signal = new ManualResetEventSlim(false);
        private static readonly ThreadStart WaitForSignalDelegate = WaitForSignal;

        public static void Main()
        {
            var processType = Environment.Is64BitProcess ? "64 bit process" : "32 bit process";
            Console.WriteLine($"Thread Size Example, running on a {processType}." );
            Console.WriteLine("------------------------------------------------");

            var numberOfThreads = 0;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                while (true)
                {
                    var newThread = new Thread(WaitForSignalDelegate);
                    newThread.Start();
                    if (!ShouldLog(++numberOfThreads))
                        continue;

                    Console.WriteLine($"After {stopwatch.Elapsed:g}");
                    Console.WriteLine($"Number of Threads: {numberOfThreads}");
                    Console.WriteLine($"Allocated Memory:  {Process.GetCurrentProcess().PrivateMemorySize64.InKiloBytes()}");
                    Console.WriteLine();
                }
            }
            catch (OutOfMemoryException)
            {
                Signal.Set();
                Console.WriteLine($"Out-Of-Memory Exception after creating {numberOfThreads} threads.");
            }

            Console.WriteLine("Press ENTER to quit...");
            Console.ReadLine();
        }

        private static bool ShouldLog(int numberOfThreads)
        {
            if (numberOfThreads < 50) return true;
            if (numberOfThreads < 400) return numberOfThreads % 50 == 0;
            if (numberOfThreads < 1000) return numberOfThreads % 100 == 0;
            if (numberOfThreads < 5000) return numberOfThreads % 500 == 0;
            if (numberOfThreads < 10000) return numberOfThreads % 1000 == 0;
            return numberOfThreads % 5000 == 0;
        }


        private static void WaitForSignal()
        {
            Signal.Wait();
        }

        private static string InKiloBytes(this long numberOfBytes)
        {
            return $"{numberOfBytes / 1024:N0} KB";
        }
    }
}

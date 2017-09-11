namespace BlockedAndRunningThreads
{
    public static class Program
    {
        public static void Main()
        {
            new ThreadPoolPerformance().RunWorkItemsOnThreadPool(shouldThreadsSleep: false)
                                       .RunWorkItemsOnThreadPool(shouldThreadsSleep: true);
        }
    }
}
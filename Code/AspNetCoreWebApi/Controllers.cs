using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWebApi
{
    [Route("api/synchronous")]
    public sealed class SynchronousController : Controller
    {
        public IActionResult Get()
        {
            Thread.Sleep(5000);
            ThreadPoolWatcher.Instance.UpdateUsedThreads();
            return Ok("Hi from synchronous method");
        }
    }

    [Route("api/asynchronous")]
    public sealed class AsynchronousController : Controller
    {
        public async Task<IActionResult> Get()
        {
            await Task.Delay(5000);
            ThreadPoolWatcher.Instance.UpdateUsedThreads();
            return Ok("Hi from asynchronous method");
        }
    }

    [Route("api/threadingResults")]
    public sealed class ThreadingResultsController : Controller
    {
        public IActionResult Get()
        {
            return Ok(new
                      {
                          ThreadPoolWatcher.Instance.MaximumWorkerThreads,
                          ThreadPoolWatcher.Instance.MaximumCompletionPortThreads,
                          ThreadPoolWatcher.Instance.UsedWorkerThreads,
                          ThreadPoolWatcher.Instance.UsedCompletionPortThreads
                      });
        }
    }
}
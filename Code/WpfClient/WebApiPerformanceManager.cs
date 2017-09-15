using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace WpfClient
{
    public sealed class WebApiPerformanceManager
    {
        private readonly HttpClient _httpClient;
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private string _currentUrl;

        public WebApiPerformanceManager(HttpClient httpClient)
        {
            _httpClient = httpClient.MustNotBeNull(nameof(httpClient));
        }

        public async Task<WebApiPerformanceResults> MeasureApiCallsAsync(bool isCallingAsynchronousApi, int numberOfCalls)
        {
            numberOfCalls.MustBeGreaterThan(0, nameof(numberOfCalls));

            _currentUrl = isCallingAsynchronousApi ? "http://localhost:51000/api/asynchronous" : "http://localhost:51000/api/synchronous";

            _stopWatch.Restart();
            var tasks = new Task<bool>[numberOfCalls];
            for (var i = 0; i < numberOfCalls; i++)
            {
                tasks[i] = CallApi();
            }

            await Task.WhenAll(tasks);
            _stopWatch.Stop();

            var response = await _httpClient.GetAsync("http://localhost:51000/api/threadingResults");
            var threadingResults = JsonConvert.DeserializeObject<WebApiThreadingResults>(await response.Content.ReadAsStringAsync());

            return new WebApiPerformanceResults(_stopWatch.Elapsed, tasks.Count(t => t.Result), tasks.Count(t => t.Result == false), threadingResults);
        }

        private async Task<bool> CallApi()
        {
            try
            {
                var response = await _httpClient.GetAsync(_currentUrl).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
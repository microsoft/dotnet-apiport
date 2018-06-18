using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PortabilityService.Functions
{
    public static class Targets
    {
        /// <summary>
        /// Returns the list of supported targets 
        /// </summary>
        /// <param name="req">Inbound request for targets</param>
        /// <param name="log">logging provider instance</param>
        /// <returns></returns>
        [FunctionName("targets")]
        public static async Task<HttpResponseMessage> Run(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "targets")] HttpRequestMessage req, ILogger log)
        {
            var targets = await GetStaticListOfTargets(log);
            var content = new StringContent(targets, Encoding.UTF8, "application/json");
            return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = content};
        }

        /// <summary>
        /// This method is a placeholder, and will be replaced with an asynchronous call to fetch catalog data in a future sprint
        /// </summary>
        /// <returns>A Task<string> representing the async task that fetches the catalog of supported targets</string></returns>
        /// <param name="log">logging provider instance</param>
        private static async Task<string> GetStaticListOfTargets(ILogger log)
        {
            return await Task.Run(() => 
            {
                var targets = string.Empty;
                try
                {
                    targets = Encoding.UTF8.GetString(Properties.Resources.ListOfTargets);
                }
                catch(Exception ex)
                {
                    log.LogError(ex.Message, ex.StackTrace);
                }
                return targets;
            });
        }
    }
}
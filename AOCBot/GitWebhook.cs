using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AOCBot
{
    public static class GitWebhook
    {
        [FunctionName("GitWebhook")]
        public static async Task Run(
            [HttpTrigger(WebHookType = "genericJson")] HttpRequestMessage req, 
            TraceWriter log)
        {
            string jsonContent = await req.Content.ReadAsStringAsync();
            log.Info($"Webhook was triggered! Content: {jsonContent}");
        }
    }
}

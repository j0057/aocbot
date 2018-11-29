using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AOCBot
{
    public static class Slack
    {
        private static string SlackToken => Environment.GetEnvironmentVariable("_AOCBOT_SLACKTOKEN");

        private static readonly HttpClient client = new HttpClient();

        static Slack()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SlackToken);
        }

        public static async Task PostMessage(string channel, string text)
        {
            var content = new StringContent(
                content: $"channel={channel}&text={text}",
                encoding: Encoding.UTF8,
                mediaType: "application/x-www-form-urlencoded");

            var response = (await client.PostAsync("https://slack.com/api/chat.postMessage", content))
                .EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            dynamic body = JObject.Parse(json);
            if (body.ok != "true")
            {
                throw new Exception((string)body.error);
            }
        }
    }
}

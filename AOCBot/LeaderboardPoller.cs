using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AOCBot
{
    public static class LeaderboardPoller
    {
        private static string SlackChannel => System.Environment.GetEnvironmentVariable("_AOCBOT_SLACKCHANNEL");

        private static async Task<TResult> TryDeserialize<TResult>(Stream stream)
            where TResult : new()
        {
            try
            {
                var json = await (new StreamReader(stream)).ReadToEndAsync();
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch
            {
                return new TResult();
            }
        }

        [FunctionName("LeaderboardPoller")]
        public static async Task Run(
            [TimerTrigger("%_AOCBOT_CRONSCHEDULE%")] TimerInfo myTimer, 
            [Blob("%_AOCBOT_BLOBNAME%", FileAccess.Read)] Stream curTimes,
            [Blob("%_AOCBOT_BLOBNAME%", FileAccess.Write)] Stream newTimes,
            TraceWriter log)
        {
            var lastTimes = await TryDeserialize<Dictionary<long, long>>(curTimes);

            var boardJSON = await AOC.GetLeaderboard();

            dynamic board = JObject.Parse(boardJSON);

            foreach (dynamic member in board.members)
            {
                var id = long.Parse((string)member.Value.id);
                var lastStarTS = long.Parse((string)member.Value.last_star_ts);
                var name = (string)member.Value.name;
                var stars = (int)member.Value.stars;
                var score = (int)member.Value.local_score;

                if (stars > 0 && (!lastTimes.ContainsKey(id) || lastTimes[id] < lastStarTS))
                {
                    var message = $"{name} now has {stars} stars and a score of {score}!";
                    log.Info($"Posting message to Slack: {message}");
                    await Slack.PostMessage(SlackChannel, message);
                }

                lastTimes[id] = lastStarTS;
            }

            var newTimesJSON = JsonConvert.SerializeObject(lastTimes);
            var newTimesBuffer = Encoding.ASCII.GetBytes(newTimesJSON);
            await newTimes.WriteAsync(newTimesBuffer, 0, newTimesBuffer.Length);
        }
    }
}

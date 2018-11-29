using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AOCBot
{
    public static class AOC
    {
        private static string AOCCookie => Environment.GetEnvironmentVariable("_AOCBOT_COOKIE");
        private static string AOCBoardURL => Environment.GetEnvironmentVariable("_AOCBOT_BOARDURL");

        private static readonly CookieContainer cookies = new CookieContainer();

        public static readonly HttpClient client = new HttpClient(new HttpClientHandler() { CookieContainer = cookies });

        static AOC()
        {
            cookies.Add(new Cookie("session", AOCCookie, "/", "adventofcode.com"));
        }

        public static async Task<string> GetLeaderboard()
        {
            var response = (await client.GetAsync(AOCBoardURL))
                .EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

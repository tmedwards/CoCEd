using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoCEd.Common
{
    public static class GitHubGetters
    {
        public static async Task<string> GetPage(string pageUrl)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(pageUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        //example: https://raw.githubusercontent.com/Ormael7/Corruption-of-Champions/0.8.46/classes/classes/GlobalFlags/kFLAGS.as
        public static async Task<string> GetContentFile(string repo, string tree, string file)
        {
            var githubRawContentUrl = @"https://raw.githubusercontent.com";
            var contentUrl = $"{githubRawContentUrl}/{repo}/{tree}/{file}";
            return await GetPage(contentUrl);
        }
        public static async Task<string> GetLatestReleaseLink(string repo)
        {
            var latestReleasePageUrl = @"https://github.com/" + repo + "/releases/latest";
            var page = await GetPage(latestReleasePageUrl);
            Regex ReleaseUrlRegex = new Regex($"<(?:a|link)\\s+href=\"((?:http\\:\\/\\/github\\.com)?\\/{repo.Replace("/", "\\/")}\\/releases\\/.*\\.(?:zip|swf|exe))\"");
            var matches = ReleaseUrlRegex.Matches(page);
            return matches[0].Groups[1].Value;
        }
        public static async Task<string> GetLatestVersion(string repo)
        {
            string releaseLink = await GetLatestReleaseLink(repo);
            var splits = releaseLink.Split('/');
            var ver_string = splits[splits.Length - 2].TrimStart('v');
            return ver_string;
        }
        public static async Task<string> GetLatestCocEdReleaseLink()
        {
            return await GetLatestReleaseLink(Settings.Default.CoCEdRepo);
        }

        public static async Task<string> GetLatestCocEdVersion()
        {
            return await GetLatestVersion(Settings.Default.CoCEdRepo);
        }
    }
}

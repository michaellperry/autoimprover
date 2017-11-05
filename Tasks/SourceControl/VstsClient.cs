using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.LuisBot.Tasks.SourceControl
{
    public class VstsClient
    {
        public async Task<List<Build>> ListBuilds()
        {
            string teamName = GetTeamName();
            string accessToken = GetAccessToken();
            string header = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{teamName}:{accessToken}"));
            string projectName = GetProjectName();
            string buildDefinitionName = GetBuildDefinition();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", header);

                string buildDefinitionsUrl = $"https://{teamName}.visualstudio.com/DefaultCollection/{projectName}/_apis/build/definitions?api-version=2.0&name={HttpUtility.UrlEncode(buildDefinitionName)}";
                string buildDefinitionsJson = await client.GetStringAsync(buildDefinitionsUrl);
                dynamic buildDefinitions = JObject.Parse(buildDefinitionsJson);

                string buildDefinitionId = buildDefinitions.value[0].id;
                string buildsUrl = $"https://{teamName}.visualstudio.com/DefaultCollection/{projectName}/_apis/build/builds?definitions={buildDefinitionId}&$top=10&api-version=2.0";
                string buildsJson = await client.GetStringAsync(buildsUrl);
                dynamic builds = JObject.Parse(buildsJson);
                JArray buildArray = (JArray)builds.value;

                return buildArray.Select((dynamic b) => new Build
                {
                    BuildNumber = b.buildNumber,
                    FinishTime = b.finishTime,
                    For = b.requestedFor.displayName,
                    Result = b.result
                }).ToList();
            }
        }

        private static string GetTeamName()
        {
            return ConfigurationManager.AppSettings["VstsTeamName"];
        }

        private static string GetProjectName()
        {
            return ConfigurationManager.AppSettings["VstsProjectName"];
        }

        private static string GetAccessToken()
        {
            return ConfigurationManager.AppSettings["VstsAccessToken"];
        }

        private static string GetBuildDefinition()
        {
            return ConfigurationManager.AppSettings["VstsBuildDefinition"];
        }
    }
}
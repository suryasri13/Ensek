using System;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Ensek.Tests.Helpers
{
    public static class BearerTokenManager
    {
        private static string token;
        private const string baseUrl = "https://qacandidatetest.ensek.io";

        public static string GetToken()
        {
            if (!string.IsNullOrEmpty(token))
                return token;

            var client = new RestClient(baseUrl);
            var request = new RestRequest("/ENSEK/login", Method.Post);
            request.AddJsonBody(new { username = "test", password = "testing" });

            var response = client.Execute(request);
            var content = JObject.Parse(response.Content);
            token = content["access_token"]?.ToString();
            return token;
        }
    }
}

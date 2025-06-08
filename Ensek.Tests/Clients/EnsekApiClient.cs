using System;
using RestSharp;
using Newtonsoft.Json.Linq;
using Ensek.Tests.Helpers;
using System.Collections.Generic;
using Ensek.Tests.Models;

namespace Ensek.Tests.Clients
{
    public class EnsekApiClient
    {
        private readonly RestClient client;
        private readonly OrderIdHelper helper;

        public EnsekApiClient()
        {
            client = new RestClient("https://qacandidatetest.ensek.io/");
            helper = new OrderIdHelper();
        }

        public RestRequest CreateRequest(string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", $"Bearer {BearerTokenManager.GetToken()}");
            return request;
        }

        public void Login(string username, string password)
        {
            var request = new RestRequest("/ENSEK/login", Method.Post);
            request.AddJsonBody(new { username, password });

            client.Execute(request);
        }

        public RestResponse ResetData()
        {
            var request = CreateRequest("/ENSEK/reset", Method.Post);
            return client.Execute(request);
        }

        public RestResponse<List<dynamic>> GetEnergies()
        {
            var request = CreateRequest("/ENSEK/energy", Method.Get);
            return client.Execute<List<dynamic>>(request);
        }

        public RestResponse<List<OrderResponse>> GetOrders()
        {
            var request = CreateRequest("/ENSEK/orders", Method.Get);
            return client.Execute<List<OrderResponse>>(request);
        }

        public (bool success, string orderId) BuyFuel(int id, int quantity)
        {
            var request = CreateRequest($"/ENSEK/buy/{id}/{quantity}", Method.Put);
            var response = client.Execute(request);
            var parsedResponse = JObject.Parse(response.Content);

            if (response.IsSuccessful)
            {
                var orderId = helper.OrderIdExtractor(parsedResponse.ToString());
                return (true, orderId);
            }

            return (false, null);
        }

        public RestResponse DeleteOrder(string orderId)
        {
            var request = CreateRequest($"/ENSEK/orders/{orderId}", Method.Delete);
            return client.Execute(request);
        }

        public RestRequest InvalidAuthRequest(string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", $"Bearer invalid");
            return request;
        }

        public RestRequest NoAuthRequest(string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method);
            return request;
        }
    }
}

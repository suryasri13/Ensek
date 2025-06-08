using NUnit.Framework;
using Ensek.Tests.Clients;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using Ensek.Tests.Helpers;

namespace Ensek.Tests
{
    public class Tests
    {
        private EnsekApiClient api;
        private List<string> createdOrderIds;
        private OrderIdHelper orderIdHelper;

        [SetUp]
        public void Setup()
        {
            api = new EnsekApiClient();
            createdOrderIds = new List<string>();
            orderIdHelper = new OrderIdHelper();
        }

        [Test, Order(1)]
        public void ResetTestData()
        {
            var response = api.ResetData();
            Assert.That(response.IsSuccessful, "ResetTestData: Reset failed.");
        }

        [Test, Order(2)]
        public void BuyFuelForAllTypes()
        {
            var response = api.GetEnergies();
            Assert.That(response.IsSuccessStatusCode, "BuyFuelForAllTypes: GET /energy failed.");

            var energies = JObject.Parse(response.Content);
            var energiesCount = energies.Properties().Count();
            Assert.That(energiesCount > 0, "BuyFuelForAllTypes: No energy types returned.");

            foreach (var energy in energies.Properties())
            {
                int remainingQuantity = (int)energy.Value["quantity_of_units"];
                int id = (int)energy.Value["energy_id"];
                if (remainingQuantity > 0)
                {
                    var (success, orderId) = api.BuyFuel(id, 1);

                    Assert.That(success, $"BuyFuelForAllTypes: Buy failed for fuel ID {id}");
                    Assert.That(orderId, Is.Not.Null.And.Not.Empty, "BuyFuelForAllTypes: Order ID missing in buy response.");

                    createdOrderIds.Add(orderId);
                }
            }
        }

        [Test, Order(3)]
        public void VerifyOrderIdsAfterBuyingArePresent()
        {
            var orders = api.GetOrders().Data;
            var orderIdsInSystem = orders.Select(o => o.Id).ToList();

            foreach (var id in createdOrderIds)
            {
                Assert.That(orderIdsInSystem, Contains.Item(id), $"VerifyOrderIdsAfterBuyingArePresent: Order ID {id} not found in /orders");
            }
        }

        [Test, Order(4)]
        public void CountOrdersBeforeNow()
        {
            var orders = api.GetOrders().Data;
            var count = orders.Count(o => o.CreatedAt < DateTime.UtcNow);

            Console.WriteLine("Orders created before now: " + count);
            Assert.That(count >= createdOrderIds.Count, "CountOrdersBeforeNow: Expected at least the created orders to be present.");
        }

        [Test, Order(5)]
        public void ValidateOrderDetails()
        {
            var orders = api.GetOrders().Data;

            foreach (var order in orders)
            {
                Assert.That(order.Quantity > 0, "Quantity must be positive.");
                Assert.That(!string.IsNullOrWhiteSpace(order.Fuel), "Fuel is missing.");
                Assert.That(order.CreatedAt <= DateTime.UtcNow, "Order has future timestamp.");
            }
        }

        [Test, Order(6)]
        public void DeleteOrder_ShouldRemoveOrder()
        {
            // Buy a valid order first
            var client = new RestClient("https://qacandidatetest.ensek.io/");
            //api.Login("test", "testing");
            var request = api.CreateRequest("/ENSEK/buy/1/1", Method.Put);
            var response = client.Execute(request);

            var parsedResponse = JObject.Parse(response.Content);
            var orderId = orderIdHelper.OrderIdExtractor(parsedResponse.ToString());           

            // Delete the order
            var deleteResponse = api.DeleteOrder(orderId);
            Assert.That(deleteResponse.IsSuccessful, "DELETE /ENSEK/orders/{id} failed.");

            // Confirm it's gone
            var ordersResponse = api.GetOrders();
            Assert.That(!ordersResponse.Content.Contains(orderId), "Order still exists after deletion.");
        }

        //Negative scenarios
        [Test, Order(7)]
        public void InvalidLogin_ShouldReturn401()
        {
            var client = new RestClient("https://qacandidatetest.ensek.io/");
            var request = new RestRequest("/ENSEK/login", Method.Post);
            request.AddJsonBody(new { username = "invalidUser", password = "invalidPassword" });

            var response = client.Execute(request);

            Assert.That((int)response.StatusCode, Is.EqualTo(401), "Expected 401 Unauthorized for invalid login.");
        }

        [Test, Order(8)]
        public void BuyFuel_InvalidEnergyId_ShouldReturn404()
        {
            var client = new RestClient("https://qacandidatetest.ensek.io/");
            api.Login("test", "testing");
            var request = api.CreateRequest("/ENSEK/order/buy/399999/1", Method.Put);

            var response = client.Execute(request);

            Assert.That(response.StatusCode == HttpStatusCode.NotFound, "Expected 404 error for invalid fuel ID.");
        }

        [Test, Order(9)]
        public void ResetData_InvalidAuthToken_ShouldReturnError()
        {
            var client = new RestClient("https://qacandidatetest.ensek.io/");
            var request = api.InvalidAuthRequest("/ENSEK/reset", Method.Post);

            var response = client.Execute(request);

            Assert.That(response.StatusCode == HttpStatusCode.Unauthorized, "Expected 401 as auth failed before calling POST /Ensek/reset");
        }

        [Test, Order(10)]
        public void ResetData_NoAuthToken_ShouldReturnError()
        {
            var client = new RestClient("https://qacandidatetest.ensek.io/");
            var request = api.NoAuthRequest("/ENSEK/reset", Method.Post);

            var response = client.Execute(request);

            Assert.That(response.StatusCode == HttpStatusCode.Unauthorized, "Expected 401 as no auth before calling POST /Ensek/reset");
        }
    }
}

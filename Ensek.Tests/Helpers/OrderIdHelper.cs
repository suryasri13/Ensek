using System;
using System.Text.RegularExpressions;
namespace Ensek.Tests.Helpers
{
    public class OrderIdHelper
    {
        public string OrderIdExtractor(string jsonMessage)
        {
            var message = Regex.Match(jsonMessage, @"""message"":\s*""([^""]+)""").Groups[1].Value;

            var match = Regex.Match(message, @"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                Console.WriteLine("Order ID not found");
                return null;
            }
        }
    }
}

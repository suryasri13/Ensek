using System;
namespace Ensek.Tests.Models
{
    public class OrderResponse
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Fuel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

using apppasteleriav04.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.Mocks
{
    public class MockSupabaseService
    {
        public List<Product> Products { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public bool ShouldFail { get; set; } = false;
        public string FailureMessage { get; set; } = "Mock error";
        public int CallCount { get; private set; } = 0;

        public Task<List<Product>> GetProductsAsync()
        {
            CallCount++;
            if (ShouldFail)
            {
                throw new Exception(FailureMessage);
            }
            return Task.FromResult(Products);
        }

        public Task<Order> CreateOrderAsync(Guid userId, List<OrderItem> items, decimal total)
        {
            CallCount++;
            if (ShouldFail)
            {
                throw new Exception(FailureMessage);
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Total = total,
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow,
                Items = items
            };

            Orders.Add(order);
            return Task.FromResult(order);
        }

        public Task<Product?> GetProductByIdAsync(Guid productId)
        {
            CallCount++;
            if (ShouldFail)
            {
                throw new Exception(FailureMessage);
            }

            var product = Products.FirstOrDefault(p => p.Id == productId);
            return Task.FromResult(product);
        }

        public Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            CallCount++;
            if (ShouldFail)
            {
                throw new Exception(FailureMessage);
            }

            var userOrders = Orders.Where(o => o.UserId == userId).ToList();
            return Task.FromResult(userOrders);
        }

        public void Reset()
        {
            Products.Clear();
            Orders.Clear();
            ShouldFail = false;
            CallCount = 0;
        }
    }
}

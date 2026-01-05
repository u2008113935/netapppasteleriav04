using apppasteleriav04.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace apppasteleriav04.Tests.Helpers
{
    public static class TestDataFactory
    {
        public static Product CreateValidProduct(string? nombre = null, decimal? precio = null)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Nombre = nombre ?? "Torta de Chocolate",
                Descripcion = "Deliciosa torta de chocolate con cobertura de ganache",
                Categoria = "Tortas",
                Precio = precio ?? 45.00m,
                ImagenPath = "torta_chocolate.jpg"
            };
        }

        public static Product CreateProductWithId(Guid id, string nombre = "Test Product", decimal precio = 10.00m)
        {
            return new Product
            {
                Id = id,
                Nombre = nombre,
                Descripcion = "Test description",
                Categoria = "Test",
                Precio = precio,
                ImagenPath = "test.jpg"
            };
        }

        public static List<Product> CreateProductList(int count)
        {
            var products = new List<Product>();
            for (int i = 0; i < count; i++)
            {
                products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    Nombre = $"Producto {i + 1}",
                    Descripcion = $"Descripción del producto {i + 1}",
                    Categoria = i % 2 == 0 ? "Tortas" : "Pasteles",
                    Precio = 10.00m + (i * 5),
                    ImagenPath = $"producto_{i + 1}.jpg"
                });
            }
            return products;
        }

        public static CartItem CreateCartItem(Guid? productId = null, string nombre = "Test Item", decimal price = 10.00m, int quantity = 1)
        {
            return new CartItem
            {
                ProductId = productId ?? Guid.NewGuid(),
                Nombre = nombre,
                ImagenPath = "test.jpg",
                Price = price,
                Quantity = quantity
            };
        }

        public static List<CartItem> CreateCartItems(int count)
        {
            var items = new List<CartItem>();
            for (int i = 0; i < count; i++)
            {
                items.Add(CreateCartItem(
                    nombre: $"Item {i + 1}",
                    price: 10.00m + i,
                    quantity: i + 1
                ));
            }
            return items;
        }

        public static Order CreateOrder(Guid? userId = null, decimal total = 100.00m, string status = "Pendiente")
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Total = total,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static OrderItem CreateOrderItem(Guid? orderId = null, Guid? productId = null, int quantity = 1, decimal price = 10.00m)
        {
            return new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId ?? Guid.NewGuid(),
                ProductId = productId ?? Guid.NewGuid(),
                Quantity = quantity,
                Price = price
            };
        }

        public static List<OrderItem> CreateOrderItems(int count, Guid? orderId = null)
        {
            var items = new List<OrderItem>();
            var orderIdToUse = orderId ?? Guid.NewGuid();
            
            for (int i = 0; i < count; i++)
            {
                items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderIdToUse,
                    ProductId = Guid.NewGuid(),
                    Quantity = i + 1,
                    Price = 10.00m + (i * 5)
                });
            }
            return items;
        }

        public static UserProfile CreateUserProfile(string email = "test@test.com", string fullName = "Test User")
        {
            return new UserProfile
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                AvatarUrl = "avatars/test_avatar.jpg"
            };
        }
    }
}

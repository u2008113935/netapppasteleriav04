using apppasteleriav04.Models.Domain;
using apppasteleriav04.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System;

namespace apppasteleriav04.Tests.UnitTests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Test_Product_Validate_WithValidData_ReturnsNoErrors()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Test_Product_Validate_WithEmptyId_ReturnsError()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            product.Id = Guid.Empty;

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().ContainSingle();
            errors.Should().Contain("Id inválido");
        }

        [Fact]
        public void Test_Product_Validate_WithEmptyName_ReturnsError()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            product.Nombre = "";

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().Contain("Nombre vacío");
        }

        [Fact]
        public void Test_Product_Validate_WithNullName_ReturnsError()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            product.Nombre = null;

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().Contain("Nombre vacío");
        }

        [Fact]
        public void Test_Product_Validate_WithNullPrice_ReturnsError()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            product.Precio = null;

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().Contain("Precio no especificado");
        }

        [Fact]
        public void Test_Product_Validate_WithNegativePrice_ReturnsError()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct();
            product.Precio = -10.00m;

            // Act
            var errors = product.Validate();

            // Assert
            errors.Should().Contain("Precio no puede ser negativo");
        }

        [Fact]
        public void Test_Product_DisplayName_WithNullName_ReturnsDefault()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = null,
                Precio = 10.00m
            };

            // Act
            var displayName = product.DisplayName;

            // Assert
            displayName.Should().Be("Producto");
        }

        [Fact]
        public void Test_Product_DisplayName_WithValidName_ReturnsName()
        {
            // Arrange
            var product = TestDataFactory.CreateValidProduct("Torta Especial");

            // Act
            var displayName = product.DisplayName;

            // Assert
            displayName.Should().Be("Torta Especial");
        }

        [Fact]
        public void Test_Product_DisplayName_WithWhitespaceName_ReturnsDefault()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = "   ",
                Precio = 10.00m
            };

            // Act
            var displayName = product.DisplayName;

            // Assert
            displayName.Should().Be("Producto");
        }
    }
}

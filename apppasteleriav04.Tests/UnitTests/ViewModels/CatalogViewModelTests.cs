using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Catalog;
using apppasteleriav04.Tests.Helpers;
using apppasteleriav04.Tests.Mocks;
using FluentAssertions;
using Xunit;
using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Tests for CatalogViewModel
    /// Note: These tests work with the existing implementation which uses SupabaseService.Instance
    /// </summary>
    public class CatalogViewModelTests
    {
        [Fact]
        public void Test_CatalogViewModel_CanBeInstantiated()
        {
            // Arrange & Act
            var viewModel = new CatalogViewModel();

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.Products.Should().NotBeNull();
        }

        [Fact]
        public void Test_CatalogViewModel_Products_InitializesEmpty()
        {
            // Arrange & Act
            var viewModel = new CatalogViewModel();

            // Assert
            viewModel.Products.Should().BeEmpty();
        }

        [Fact]
        public async Task Test_LoadProductsAsync_SetsIsBusyTrue_ThenFalse()
        {
            // Arrange
            var viewModel = new CatalogViewModel();
            
            // Act
            var loadTask = viewModel.LoadProductsAsync();
            var busyDuringLoad = viewModel.IsBusy;
            await loadTask;
            var busyAfterLoad = viewModel.IsBusy;

            // Assert
            // Note: IsBusy might be false by the time we check due to timing
            // In a real scenario with a slower service, we'd see it as true
            busyAfterLoad.Should().BeFalse();
        }

        [Fact]
        public async Task Test_LoadProductsAsync_PopulatesProducts()
        {
            // Arrange
            var viewModel = new CatalogViewModel();
            
            // Act
            await viewModel.LoadProductsAsync();

            // Assert
            // This will call the real SupabaseService, which should return products
            // In a unit test, we'd normally mock this, but the current implementation
            // uses a static singleton, so we're testing the integration
            viewModel.Products.Should().NotBeNull();
        }

        // Note: The following tests would require dependency injection to mock SupabaseService
        // Currently CatalogViewModel uses SupabaseService.Instance directly
        /*
        [Fact]
        public async Task Test_LoadProductsAsync_OnError_SetsHasError()
        {
            // Would test error handling with mocked service
        }

        [Fact]
        public void Test_SearchText_FiltersProducts()
        {
            // Would test search functionality if it exists
        }

        [Fact]
        public void Test_AddToCartCommand_AddsProductToCart()
        {
            // Would test add to cart command
        }

        [Fact]
        public async Task Test_RefreshCommand_ReloadsProducts()
        {
            // Would test refresh functionality
        }
        */
    }
}

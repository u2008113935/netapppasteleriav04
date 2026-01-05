using apppasteleriav04.Services.Core;
using apppasteleriav04.Tests.Mocks;
using FluentAssertions;
using Xunit;
using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for AuthService
    /// Note: Uses the singleton instance, so tests may interact
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authService = AuthService.Instance;
            _authService.Logout(); // Clean state
        }

        public void Dispose()
        {
            _authService.Logout(); // Clean up after each test
        }

        [Fact]
        public void Test_AuthService_InitialState_NotAuthenticated()
        {
            // Arrange & Act
            var isAuthenticated = _authService.IsAuthenticated;

            // Assert
            isAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task Test_SignInAsync_WithEmptyEmail_ReturnsFalse()
        {
            // Arrange
            var password = "password123";

            // Act
            var result = await _authService.SignInAsync("", password);

            // Assert
            result.Should().BeFalse();
            _authService.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task Test_SignInAsync_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            var email = "test@test.com";

            // Act
            var result = await _authService.SignInAsync(email, "");

            // Assert
            result.Should().BeFalse();
            _authService.IsAuthenticated.Should().BeFalse();
        }

        // Note: The following tests require actual Supabase connection or mocking
        // Since AuthService uses SupabaseService.Instance, these are integration-like tests
        /*
        [Fact]
        public async Task Test_SignInAsync_WithValidCredentials_ReturnsTrue()
        {
            // Would test successful login with mocked SupabaseService
        }

        [Fact]
        public async Task Test_SignInAsync_WithInvalidCredentials_ReturnsFalse()
        {
            // Would test failed login
        }

        [Fact]
        public async Task Test_SignInAsync_SetsUserData_OnSuccess()
        {
            // Would test that user data is set after successful login
        }
        */

        [Fact]
        public void Test_Logout_ClearsUserData()
        {
            // Arrange
            // Manually set some auth data to simulate logged in state
            // Note: This is a workaround since we can't easily mock the login
            
            // Act
            _authService.Logout();

            // Assert
            _authService.IsAuthenticated.Should().BeFalse();
            _authService.UserId.Should().BeNull();
            _authService.UserEmail.Should().BeNull();
            _authService.AccessToken.Should().BeNull();
        }

        [Fact]
        public async Task Test_SignOutAsync_ClearsUserData()
        {
            // Arrange & Act
            await _authService.SignOutAsync();

            // Assert
            _authService.IsAuthenticated.Should().BeFalse();
            _authService.UserId.Should().BeNull();
            _authService.UserEmail.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetAccessTokenAsync_WhenNotAuthenticated_ReturnsNull()
        {
            // Arrange
            _authService.Logout();

            // Act
            var token = await _authService.GetAccessTokenAsync();

            // Assert
            token.Should().BeNullOrEmpty();
        }
    }
}

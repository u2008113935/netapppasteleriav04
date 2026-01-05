using System;
using System.Threading.Tasks;

namespace apppasteleriav04.Tests.Mocks
{
    public class MockAuthService
    {
        public bool IsAuthenticated { get; set; } = false;
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? AccessToken { get; set; }
        public bool ShouldFail { get; set; } = false;
        public string FailureMessage { get; set; } = "Authentication failed";

        // Test credentials
        public const string ValidEmail = "test@test.com";
        public const string ValidPassword = "123456";

        public Task<bool> SignInAsync(string email, string password)
        {
            if (ShouldFail)
            {
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult(false);
            }

            if (email == ValidEmail && password == ValidPassword)
            {
                IsAuthenticated = true;
                UserId = Guid.NewGuid().ToString();
                UserEmail = email;
                AccessToken = "mock_access_token_" + Guid.NewGuid().ToString();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> SignUpAsync(string email, string password, string name, string? phone = null)
        {
            if (ShouldFail)
            {
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult(false);
            }

            IsAuthenticated = true;
            UserId = Guid.NewGuid().ToString();
            UserEmail = email;
            AccessToken = "mock_access_token_" + Guid.NewGuid().ToString();
            return Task.FromResult(true);
        }

        public void SignOut()
        {
            IsAuthenticated = false;
            UserId = null;
            UserEmail = null;
            AccessToken = null;
        }

        public Task SignOutAsync()
        {
            SignOut();
            return Task.CompletedTask;
        }

        public Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult(AccessToken);
        }

        public void SetAuthenticated(string userId, string email)
        {
            IsAuthenticated = true;
            UserId = userId;
            UserEmail = email;
            AccessToken = "mock_access_token_" + Guid.NewGuid().ToString();
        }

        public void Reset()
        {
            IsAuthenticated = false;
            UserId = null;
            UserEmail = null;
            AccessToken = null;
            ShouldFail = false;
        }
    }
}

using System;

namespace apppasteleriav04.ViewModels.Auth
{
    public class LoginCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Email { get; set; }
    }
}

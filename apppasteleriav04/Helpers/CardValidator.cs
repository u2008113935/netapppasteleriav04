using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace apppasteleriav04.Helpers
{
    public static class CardValidator
    {
        /// <summary>
        /// Validates a credit card number using the Luhn algorithm
        /// </summary>
        public static bool ValidateLuhn(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Remove spaces and non-digits
            cardNumber = Regex.Replace(cardNumber, @"\D", "");

            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            // Process digits from right to left
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }

        /// <summary>
        /// Gets the card brand based on the card number
        /// </summary>
        public static string GetCardBrand(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return "Unknown";

            cardNumber = Regex.Replace(cardNumber, @"\D", "");

            // Visa
            if (Regex.IsMatch(cardNumber, @"^4"))
                return "Visa";

            // Mastercard
            if (Regex.IsMatch(cardNumber, @"^(5[1-5]|2[2-7])"))
                return "Mastercard";

            // American Express
            if (Regex.IsMatch(cardNumber, @"^3[47]"))
                return "Amex";

            // Discover
            if (Regex.IsMatch(cardNumber, @"^6(?:011|5)"))
                return "Discover";

            // Diners Club
            if (Regex.IsMatch(cardNumber, @"^3(?:0[0-5]|[68])"))
                return "Diners";

            return "Unknown";
        }

        /// <summary>
        /// Formats card number with spaces (e.g., 1234 5678 9012 3456)
        /// </summary>
        public static string FormatCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return string.Empty;

            cardNumber = Regex.Replace(cardNumber, @"\D", "");

            // Format in groups of 4
            var formatted = string.Join(" ", Enumerable.Range(0, (cardNumber.Length + 3) / 4)
                .Select(i => cardNumber.Substring(i * 4, Math.Min(4, cardNumber.Length - i * 4))));

            return formatted;
        }

        /// <summary>
        /// Validates expiry date (MM/YY format)
        /// </summary>
        public static bool ValidateExpiry(string expiry)
        {
            if (string.IsNullOrWhiteSpace(expiry))
                return false;

            // Check format MM/YY
            if (!Regex.IsMatch(expiry, @"^\d{2}/\d{2}$"))
                return false;

            var parts = expiry.Split('/');
            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            // Convert YY to full year (20YY)
            int fullYear = 2000 + year;

            var expiryDate = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));
            return expiryDate >= DateTime.Now;
        }

        /// <summary>
        /// Validates CVV code
        /// </summary>
        public static bool ValidateCvv(string cvv, string cardBrand)
        {
            if (string.IsNullOrWhiteSpace(cvv))
                return false;

            cvv = Regex.Replace(cvv, @"\D", "");

            // Amex uses 4 digits, others use 3
            if (cardBrand?.ToLower() == "amex")
                return cvv.Length == 4;

            return cvv.Length == 3;
        }

        /// <summary>
        /// Masks a card number, showing only last 4 digits
        /// </summary>
        public static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return string.Empty;

            cardNumber = Regex.Replace(cardNumber, @"\D", "");

            if (cardNumber.Length < 4)
                return new string('*', cardNumber.Length);

            return new string('*', cardNumber.Length - 4) + cardNumber.Substring(cardNumber.Length - 4);
        }

        /// <summary>
        /// Gets last 4 digits of card number
        /// </summary>
        public static string GetLastFour(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return string.Empty;

            cardNumber = Regex.Replace(cardNumber, @"\D", "");

            return cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
        }
    }
}

using System.Text.RegularExpressions;

namespace Project.Validators
{
    public static class Validator
    {
        private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public static bool ValidEmail(string email)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, EmailRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (RegexMatchTimeoutException)
            {
            }
            return false;
        }

        public static bool ValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) &&
                       phone.Length >= 9 &&
                       Regex.IsMatch(phone, @"^[0-9\-]+$");
        }
        public static bool ValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        public static bool ValidateData(string name, string email, string phone)
        {
            if (!ValidName(name) || !ValidEmail(email) || !ValidPhone(phone))
                return false;
            return true;
        }
    }
}

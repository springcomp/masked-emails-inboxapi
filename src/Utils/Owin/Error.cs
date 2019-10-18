using System;
using System.Collections.Generic;

namespace WebApi.Services
{
    public sealed class Error
    {
        public static Exception NoSuchMaskedEmailAddress(string email)
        {
            return new KeyNotFoundException($"No such masked email address: '{email}'.");
        }

        public static Exception NoSuchProfile(string userId)
        {
            return new KeyNotFoundException("No user matching the specified identifier was found.");
        }

        public string Reason { get; set; }
    }
}
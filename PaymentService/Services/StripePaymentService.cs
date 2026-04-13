using PaymentService.Interfaces;
using System;
using System.Threading.Tasks;

namespace PaymentService.Services
{
    /// <summary>
    /// Stripe payment integration stub.
    /// Replace the body of each method with real Stripe SDK calls once
    /// the Stripe.net NuGet package is added to the project.
    /// </summary>
    public class StripePaymentService : IStripePaymentService
    {
        public async Task<string> CreatePaymentAsync(decimal amount, string currency, string sourceToken)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
            if (string.IsNullOrWhiteSpace(sourceToken)) throw new ArgumentException("Source token is required.", nameof(sourceToken));

            // TODO: Replace with real Stripe SDK call:
            // var options = new ChargeCreateOptions { Amount = (long)(amount * 100), Currency = currency, Source = sourceToken };
            // var service = new ChargeService();
            // var charge = await service.CreateAsync(options);
            // return charge.Id;

            await Task.CompletedTask;
            throw new NotImplementedException("Stripe integration is not yet configured. Add Stripe.net NuGet package and set StripeSecretKey in appsettings.");
        }

        public async Task<bool> RefundPaymentAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

            // TODO: Replace with real Stripe SDK call:
            // var options = new RefundCreateOptions { Charge = transactionId };
            // var service = new RefundService();
            // var refund = await service.CreateAsync(options);
            // return refund.Status == "succeeded";

            await Task.CompletedTask;
            throw new NotImplementedException("Stripe integration is not yet configured. Add Stripe.net NuGet package and set StripeSecretKey in appsettings.");
        }
    }
}

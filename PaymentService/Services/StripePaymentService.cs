using PaymentService.Interfaces;
using System;
using System.Threading.Tasks;

namespace PaymentService.Services
{
	public class StripePaymentService : IStripePaymentService
	{
		public async Task<string> CreatePaymentAsync(decimal amount, string currency, string sourceToken)
		{
			// TODO: Integrate Stripe API here

			await Task.Delay(100); // simulate async call

			return Guid.NewGuid().ToString(); // fake transaction id
		}

		public async Task<bool> RefundPaymentAsync(string transactionId)
		{
			// TODO: Integrate Stripe refund API

			await Task.Delay(100);

			return true; // assume success
		}
	}
}
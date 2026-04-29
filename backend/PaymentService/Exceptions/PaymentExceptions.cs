namespace PaymentService.Exceptions
{
    public class PaymentNotFoundException : Exception
    {
        public PaymentNotFoundException(int paymentId)
            : base($"Payment #{paymentId} was not found.") { }
    }

    public class PaymentAlreadyRefundedException : Exception
    {
        public PaymentAlreadyRefundedException(int paymentId)
            : base($"Payment #{paymentId} has already been refunded.") { }
    }

    public class InvalidPaymentMethodException : Exception
    {
        public InvalidPaymentMethodException(string method)
            : base($"'{method}' is not a supported payment method. Use 'COD' or 'Card'.") { }
    }

    public class PaymentAmountException : Exception
    {
        public PaymentAmountException()
            : base("Payment amount must be greater than zero.") { }
    }
}

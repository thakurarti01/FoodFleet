namespace OrderService.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(int orderId)
            : base($"Order #{orderId} was not found.") { }
    }

    public class OrderAlreadyCancelledException : Exception
    {
        public OrderAlreadyCancelledException(int orderId)
            : base($"Order #{orderId} has already been cancelled.") { }
    }

    public class OrderNotCancellableException : Exception
    {
        public OrderNotCancellableException(int orderId, string status)
            : base($"Order #{orderId} cannot be cancelled because it is already '{status}'.") { }
    }

    public class OrderAlreadyDeliveredException : Exception
    {
        public OrderAlreadyDeliveredException(int orderId)
            : base($"Order #{orderId} has already been delivered.") { }
    }

    public class EmptyOrderException : Exception
    {
        public EmptyOrderException()
            : base("An order must contain at least one item.") { }
    }

    public class InvalidOrderStatusException : Exception
    {
        public InvalidOrderStatusException(string status)
            : base($"'{status}' is not a valid order status.") { }
    }
}

namespace RestaurantService.Exceptions
{
    public class RestaurantNotFoundException : Exception
    {
        public RestaurantNotFoundException(Guid id)
            : base($"Restaurant '{id}' was not found.") { }
    }

    public class RestaurantNotApprovedException : Exception
    {
        public RestaurantNotApprovedException(string name)
            : base($"Restaurant '{name}' has not been approved yet.") { }
    }

    public class RestaurantClosedException : Exception
    {
        public RestaurantClosedException(string name)
            : base($"Restaurant '{name}' is currently closed and not accepting orders.") { }
    }

    public class MenuItemNotFoundException : Exception
    {
        public MenuItemNotFoundException(int itemId)
            : base($"Menu item #{itemId} was not found.") { }
    }

    public class ReviewNotFoundException : Exception
    {
        public ReviewNotFoundException(int reviewId)
            : base($"Review #{reviewId} was not found.") { }
    }

    public class DuplicateReviewException : Exception
    {
        public DuplicateReviewException(int orderId)
            : base($"A review for order #{orderId} already exists.") { }
    }
}

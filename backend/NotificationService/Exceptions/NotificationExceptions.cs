namespace NotificationService.Exceptions
{
    public class NotificationNotFoundException : Exception
    {
        public NotificationNotFoundException(Guid id)
            : base($"Notification '{id}' was not found.") { }
    }

    public class EmailDeliveryException : Exception
    {
        public EmailDeliveryException(string recipient, string reason)
            : base($"Failed to deliver email to '{recipient}': {reason}") { }
    }
}

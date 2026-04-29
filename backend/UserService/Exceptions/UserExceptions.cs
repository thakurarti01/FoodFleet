namespace UserService.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string email)
            : base($"A user with email '{email}' already exists.") { }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string identifier)
            : base($"User '{identifier}' was not found.") { }
    }

    public class WeakPasswordException : Exception
    {
        public WeakPasswordException()
            : base("Password must be at least 6 characters long.") { }
    }

    public class AccountDeactivatedException : Exception
    {
        public AccountDeactivatedException(string email)
            : base($"Account '{email}' has been deactivated. Contact support.") { }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid email or password.") { }
    }

    public class InvalidResetTokenException : Exception
    {
        public InvalidResetTokenException()
            : base("Password reset token is invalid or has expired.") { }
    }
}

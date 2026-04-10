using System;

namespace ReviewService.Exceptions
{
	// Thrown when a review already exists for an order by the same customer (FR-REV-06)
	public class ReviewAlreadyExistsException : Exception
	{
		public ReviewAlreadyExistsException() : base("You have already submitted a review for this order.") { }
		public ReviewAlreadyExistsException(string message) : base(message) { }
	}

	// Thrown when a review is not found
	public class ReviewNotFoundException : Exception
	{
		public ReviewNotFoundException() : base("Review not found.") { }
		public ReviewNotFoundException(string message) : base(message) { }
	}

	// Thrown when a restaurant owner tries to respond to a non-existing or deleted review
	public class InvalidOwnerResponseException : Exception
	{
		public InvalidOwnerResponseException() : base("Cannot respond to this review.") { }
		public InvalidOwnerResponseException(string message) : base(message) { }
	}

	// Thrown when an admin tries to delete a non-existing or already deleted review
	public class ReviewDeletionException : Exception
	{
		public ReviewDeletionException() : base("Review cannot be deleted.") { }
		public ReviewDeletionException(string message) : base(message) { }
	}
}
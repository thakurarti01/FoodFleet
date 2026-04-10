using ReviewService.DTOs;
using ReviewService.Models;

namespace ReviewService.Interfaces
{
	public interface IReviewService
	{
		// Customer submits a review
		Task<Review> CreateReviewAsync(CreateReviewDTO dto);

		// Get all reviews of a restaurant
		Task<IEnumerable<Review>> GetReviewsByRestaurantAsync(int restaurantId);

		// Get a review by order (for single review check)
		Task<Review?> GetReviewByOrderAsync(int orderId);

		// Customer updates their review
		Task<Review?> UpdateReviewAsync(int reviewId, UpdateReviewDTO dto);

		// Admin deletes a review (soft delete)
		Task<bool> DeleteReviewAsync(int reviewId);

		// Owner responds to review
		Task<Review?> AddOwnerResponseAsync(OwnerResponseDTO dto);

		// Calculate average rating of restaurant
		Task<double> CalculateAverageRatingAsync(int restaurantId);
	}
}
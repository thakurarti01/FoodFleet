using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.DTOs;
using ReviewService.Exceptions;
using ReviewService.Interfaces;
using ReviewService.Models;

namespace ReviewService.Services
{
	public class ReviewServiceImp : IReviewService
	{
		private readonly ReviewDbContext _context;

		public ReviewServiceImp(ReviewDbContext context)
		{
			_context = context;
		}

		// Create a new review (FR-REV-01, FR-REV-02, FR-REV-06)
		public async Task<Review> CreateReviewAsync(CreateReviewDTO dto)
		{
			// Check if review already exists for this order by this customer
			var existing = await _context.Reviews
				.FirstOrDefaultAsync(r => r.OrderId == dto.OrderId && r.CustomerId == dto.CustomerId);

			if (existing != null)
			{
				throw new ReviewAlreadyExistsException();
			}
			

			var review = new Review
			{
				OrderId = dto.OrderId,
				CustomerId = dto.CustomerId,
				RestaurantId = dto.RestaurantId,
				Rating = dto.Rating,
				Comment = dto.Comment,
				CreatedAt = DateTime.UtcNow
			};

			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();

			return review;
		}

		// Get all reviews of a restaurant
		public async Task<IEnumerable<Review>> GetReviewsByRestaurantAsync(int restaurantId)
		{
			return await _context.Reviews
				.Where(r => r.RestaurantId == restaurantId && !r.IsDeleted)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
		}

		// Get review by order
		public async Task<Review?> GetReviewByOrderAsync(int orderId)
		{
			return await _context.Reviews
				.FirstOrDefaultAsync(r => r.OrderId == orderId && !r.IsDeleted);
		}

		// Update review
		public async Task<Review?> UpdateReviewAsync(int reviewId, UpdateReviewDTO dto)
		{
			var review = await _context.Reviews.FindAsync(reviewId);
			if (review == null || review.IsDeleted)
			{
				throw new ReviewNotFoundException();
			}
				

			if (dto.Rating.HasValue) review.Rating = dto.Rating.Value;
			if (!string.IsNullOrEmpty(dto.Comment)) review.Comment = dto.Comment;

			await _context.SaveChangesAsync();
			return review;
		}

		// Admin deletes a review
		public async Task<bool> DeleteReviewAsync(int reviewId)
		{
			var review = await _context.Reviews.FindAsync(reviewId);
			if (review == null || review.IsDeleted)
			{
				throw new ReviewDeletionException();
			}
				

			review.IsDeleted = true;
			await _context.SaveChangesAsync();
			return true;
		}

		// Owner responds to a review
		public async Task<Review?> AddOwnerResponseAsync(OwnerResponseDTO dto)
		{
			var review = await _context.Reviews.FindAsync(dto.ReviewId);
			if (review == null || review.IsDeleted)
			{
				throw new InvalidOwnerResponseException();
			}
				

			review.OwnerResponse = dto.ResponseText;
			review.ResponseCreatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return review;
		}

		// Calculate average rating for a restaurant (FR-REV-03)
		public async Task<double> CalculateAverageRatingAsync(int restaurantId)
		{
			var reviews = await _context.Reviews
				.Where(r => r.RestaurantId == restaurantId && !r.IsDeleted)
				.ToListAsync();

			if (!reviews.Any())
			{
				return 0;
			}

			return reviews.Average(r => r.Rating);
		}
	}
}
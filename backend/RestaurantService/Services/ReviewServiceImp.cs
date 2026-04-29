using Microsoft.EntityFrameworkCore;
using RestaurantService.Data;
using RestaurantService.DTOs;
using RestaurantService.Exceptions;
using RestaurantService.Interfaces;
using RestaurantService.Models;

namespace RestaurantService.Services
{
    /// <summary>
    /// Manages customer reviews for restaurants.
    /// One review per customer per order; supports owner responses and admin soft-delete.
    /// </summary>
    public class ReviewServiceImp : IReviewService
    {
        private readonly RestaurantDbContext _context;

        public ReviewServiceImp(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetByRestaurantAsync(Guid restaurantId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.RestaurantId == restaurantId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve reviews: {ex.Message}", ex);
            }
        }

        public async Task<Review?> GetByOrderAsync(int orderId)
        {
            try
            {
                return await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == orderId && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve review: {ex.Message}", ex);
            }
        }

        public async Task<Review> CreateAsync(Guid restaurantId, CreateReviewDto dto)
        {
            try
            {
                // Check if review already exists for this order and customer
                var existing = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId && r.CustomerId == dto.CustomerId);

                if (existing != null)
                    throw new DuplicateReviewException(dto.OrderId);

                var review = new Review
                {
                    RestaurantId = restaurantId,
                    CustomerId = dto.CustomerId,
                    OrderId = dto.OrderId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                return review;
            }
            catch (DuplicateReviewException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create review: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(int reviewId, UpdateReviewDto dto, Guid customerId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId)
                    ?? throw new ReviewNotFoundException(reviewId);

                if (review.IsDeleted || review.CustomerId != customerId)
                    throw new ReviewNotFoundException(reviewId);

                if (dto.Rating.HasValue) review.Rating = dto.Rating.Value;
                if (dto.Comment != null) review.Comment = dto.Comment;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (ReviewNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update review: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId)
                    ?? throw new ReviewNotFoundException(reviewId);

                review.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (ReviewNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete review: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddOwnerResponseAsync(int reviewId, OwnerResponseDto dto)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId)
                    ?? throw new ReviewNotFoundException(reviewId);

                if (review.IsDeleted)
                    throw new ReviewNotFoundException(reviewId);

                review.OwnerResponse = dto.ResponseText;
                review.ResponseCreatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (ReviewNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add owner response: {ex.Message}", ex);
            }
        }

        public async Task<double> GetAverageRatingAsync(Guid restaurantId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.RestaurantId == restaurantId && !r.IsDeleted)
                    .ToListAsync();
                return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to calculate average rating: {ex.Message}", ex);
            }
        }
    }
}

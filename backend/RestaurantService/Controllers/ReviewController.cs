using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTOs;
using RestaurantService.Exceptions;
using RestaurantService.Interfaces;
using System.Security.Claims;

namespace RestaurantService.Controllers
{
    /// <summary>
    /// REST endpoints for restaurant reviews.
    /// Customers can create/update reviews; owners can respond; admins can delete.
    /// Average rating is publicly accessible without authentication.
    /// </summary>
    [ApiController]
    [Route("api/restaurants")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{restaurantId}/reviews")]
        public async Task<IActionResult> GetReviews(Guid restaurantId)
            => Ok(await _reviewService.GetByRestaurantAsync(restaurantId));

        [HttpGet("{restaurantId}/reviews/average-rating")]
        public async Task<IActionResult> GetAverageRating(Guid restaurantId)
        {
            var avg = await _reviewService.GetAverageRatingAsync(restaurantId);
            return Ok(new { restaurantId, averageRating = avg });
        }

        [HttpGet("reviews/order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var review = await _reviewService.GetByOrderAsync(orderId);
            return review == null ? NotFound() : Ok(review);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{restaurantId}/reviews")]
        public async Task<IActionResult> CreateReview(Guid restaurantId, CreateReviewDto dto)
        {
            try
            {
                var review = await _reviewService.CreateAsync(restaurantId, dto);
                return Ok(review);
            }
            catch (DuplicateReviewException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to create review: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("reviews/{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, UpdateReviewDto dto)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.UpdateAsync(reviewId, dto, customerId);
            return result ? Ok("Updated") : NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("reviews/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewService.DeleteAsync(reviewId);
            return result ? Ok("Deleted") : NotFound();
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("reviews/{reviewId}/respond")]
        public async Task<IActionResult> OwnerRespond(int reviewId, OwnerResponseDto dto)
        {
            var result = await _reviewService.AddOwnerResponseAsync(reviewId, dto);
            return result ? Ok("Response added") : NotFound();
        }
    }
}

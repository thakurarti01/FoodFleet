using Microsoft.AspNetCore.Mvc;
using ReviewService.DTOs;
using ReviewService.Exceptions;
using ReviewService.Interfaces;
using ReviewService.Models;

namespace ReviewService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;

		public ReviewController(IReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		// ------------------- Customer: Submit Review -------------------
		[HttpPost]
		public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO dto)
		{
			try
			{
				var review = await _reviewService.CreateReviewAsync(dto);
				return CreatedAtAction(nameof(GetReviewByOrder), new { orderId = review.OrderId }, review);
			}
			catch (ReviewAlreadyExistsException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ------------------- Get Review by Order -------------------
		[HttpGet("order/{orderId}")]
		public async Task<IActionResult> GetReviewByOrder(int orderId)
		{
			var review = await _reviewService.GetReviewByOrderAsync(orderId);
			if (review == null)
				return NotFound(new { message = "Review not found." });

			return Ok(review);
		}

		// ------------------- Get Reviews for a Restaurant -------------------
		[HttpGet("restaurant/{restaurantId}")]
		public async Task<IActionResult> GetReviewsByRestaurant(int restaurantId)
		{
			var reviews = await _reviewService.GetReviewsByRestaurantAsync(restaurantId);
			return Ok(reviews);
		}

		// ------------------- Customer: Update Review -------------------
		[HttpPut("{reviewId}")]
		public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDTO dto)
		{
			try
			{
				var updated = await _reviewService.UpdateReviewAsync(reviewId, dto);
				return Ok(updated);
			}
			catch (ReviewNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}

		// ------------------- Admin: Delete Review -------------------
		[HttpDelete("{reviewId}")]
		public async Task<IActionResult> DeleteReview(int reviewId)
		{
			try
			{
				var result = await _reviewService.DeleteReviewAsync(reviewId);
				return Ok(new { message = "Review deleted successfully." });
			}
			catch (ReviewDeletionException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ------------------- Owner: Respond to Review -------------------
		[HttpPost("response")]
		public async Task<IActionResult> AddOwnerResponse([FromBody] OwnerResponseDTO dto)
		{
			try
			{
				var review = await _reviewService.AddOwnerResponseAsync(dto);
				return Ok(review);
			}
			catch (InvalidOwnerResponseException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// ------------------- Get Average Rating -------------------
		[HttpGet("restaurant/{restaurantId}/average-rating")]
		public async Task<IActionResult> GetAverageRating(int restaurantId)
		{
			var avgRating = await _reviewService.CalculateAverageRatingAsync(restaurantId);
			return Ok(new { restaurantId, averageRating = avgRating });
		}
	}
}
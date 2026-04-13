using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Interfaces;
using AdminService.DTOs;
using AdminService.Helpers;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]  // only Admin role can access
    public class AdminController : ControllerBase
	{
		private readonly IAdminService _service;

		public AdminController(IAdminService service)
		{
			_service = service;
		}

		//  Users / Restaurants / Agents

		[HttpGet("users")]
		public async Task<IActionResult> GetUsers()
		{
			var data = await _service.GetAllUsersAsync();
			return Ok(new ApiResponse<object>(true, "Users fetched", data));
		}

		[HttpGet("restaurants")]
		public async Task<IActionResult> GetRestaurants()
		{
			var data = await _service.GetAllRestaurantsAsync();
			return Ok(new ApiResponse<object>(true, "Restaurants fetched", data));
		}

		[HttpGet("agents")]
		public async Task<IActionResult> GetAgents()
		{
			var data = await _service.GetAllAgentsAsync();
			return Ok(new ApiResponse<object>(true, "Agents fetched", data));
		}

		[HttpDelete("user/{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			var result = await _service.DeleteUserAsync(id);
			return Ok(new ApiResponse<bool>(true, "User deleted", result));
		}

		//  Approve / Reject Restaurant

		[HttpPut("restaurant/approve")]
		public async Task<IActionResult> ApproveRestaurant(ApproveRestaurantDTO dto)
		{
			var result = await _service.ApproveRestaurantAsync(dto.RestaurantId);
			return Ok(new ApiResponse<bool>(true, "Restaurant approved", result));
		}

		[HttpPut("restaurant/reject")]
		public async Task<IActionResult> RejectRestaurant(RejectRestaurantDTO dto)
		{
			var result = await _service.RejectRestaurantAsync(dto.RestaurantId, dto.Reason);
			return Ok(new ApiResponse<bool>(true, "Restaurant rejected", result));
		}

		//  Analytics

		[HttpGet("analytics")]
		public async Task<IActionResult> GetAnalytics()
		{
			var data = await _service.GetAnalyticsAsync();
			return Ok(new ApiResponse<object>(true, "Analytics fetched", data));
		}

		//  Disputes & Refund

		[HttpGet("disputes")]
		public async Task<IActionResult> GetDisputes()
		{
			var data = await _service.GetAllDisputesAsync();
			return Ok(new ApiResponse<object>(true, "Disputes fetched", data));
		}

		[HttpPost("refund")]
		public async Task<IActionResult> ProcessRefund(RefundDTO dto)
		{
			var data = await _service.ProcessRefundAsync(dto);
			return Ok(new ApiResponse<object>(true, "Refund processed", data));
		}

		//  Banner & Featured

		[HttpPost("banner")]
		public async Task<IActionResult> CreateBanner(BannerDTO dto)
		{
			var data = await _service.CreateBannerAsync(dto);
			return Ok(new ApiResponse<object>(true, "Banner created", data));
		}

		[HttpGet("banner")]
		public async Task<IActionResult> GetBanners()
		{
			var data = await _service.GetAllBannersAsync();
			return Ok(new ApiResponse<object>(true, "Banners fetched", data));
		}

		[HttpPost("featured/{restaurantId}")]
		public async Task<IActionResult> AddFeatured(int restaurantId)
		{
			var data = await _service.AddFeaturedRestaurantAsync(restaurantId);
			return Ok(new ApiResponse<object>(true, "Added to featured", data));
		}

		//  CSV Reports

		[HttpGet("reports/orders")]
		public async Task<IActionResult> ExportOrders(DateTime startDate, DateTime endDate)
		{
			var file = await _service.ExportOrdersReportAsync(startDate, endDate);
			return File(file, "text/csv", "orders_report.csv");
		}

		[HttpGet("reports/revenue")]
		public async Task<IActionResult> ExportRevenue(DateTime startDate, DateTime endDate)
		{
			var file = await _service.ExportRevenueReportAsync(startDate, endDate);
			return File(file, "text/csv", "revenue_report.csv");
		}
	}
}
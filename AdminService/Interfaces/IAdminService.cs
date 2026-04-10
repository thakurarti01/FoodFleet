using AdminService.DTOs;
using AdminService.Models;

namespace AdminService.Interfaces
{
	public interface IAdminService
	{
		//  Manage Users / Restaurants / Agents
		Task<IEnumerable<User>> GetAllUsersAsync();
		Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
		Task<IEnumerable<DeliveryAgent>> GetAllAgentsAsync();
		Task<bool> DeleteUserAsync(int userId);

		//  Approve / Reject Restaurant
		Task<bool> ApproveRestaurantAsync(int restaurantId);
		Task<bool> RejectRestaurantAsync(int restaurantId, string reason);

		//  Analytics
		Task<AnalyticsDTO> GetAnalyticsAsync();

		//  Disputes & Refunds
		Task<IEnumerable<Dispute>> GetAllDisputesAsync();
		Task<Refund> ProcessRefundAsync(RefundDTO refundDTO);

		//  Banners & Featured Restaurants
		Task<Banner> CreateBannerAsync(BannerDTO bannerDTO);
		Task<IEnumerable<Banner>> GetAllBannersAsync();
		Task<FeaturedRestaurant> AddFeaturedRestaurantAsync(int restaurantId);

		// Export Reports (CSV)
		Task<byte[]> ExportOrdersReportAsync(DateTime startDate, DateTime endDate);
		Task<byte[]> ExportRevenueReportAsync(DateTime startDate, DateTime endDate);
	}
}
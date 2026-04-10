using AdminService.Data;
using AdminService.DTOs;
using AdminService.Interfaces;
using AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Services
{
	public class AdminServiceImp : IAdminService
	{
		private readonly AdminDbContext _context;

		public AdminServiceImp(AdminDbContext context)
		{
			_context = context;
		}

		// Users / Restaurants / Agents
		public async Task<IEnumerable<User>> GetAllUsersAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync()
		{
			return await _context.Restaurants.ToListAsync();
		}

		public async Task<IEnumerable<DeliveryAgent>> GetAllAgentsAsync()
		{
			return await _context.DeliveryAgents.ToListAsync();
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null) return false;

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
			return true;
		}

		//  Approve / Reject Restaurant
		public async Task<bool> ApproveRestaurantAsync(int restaurantId)
		{
			var restaurant = await _context.Restaurants.FindAsync(restaurantId);
			if (restaurant == null) return false;

			restaurant.IsApproved = true;
			restaurant.IsRejected = false;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> RejectRestaurantAsync(int restaurantId, string reason)
		{
			var restaurant = await _context.Restaurants.FindAsync(restaurantId);
			if (restaurant == null) return false;

			restaurant.IsApproved = false;
			restaurant.IsRejected = true;

			await _context.SaveChangesAsync();
			return true;
		}

		//  Analytics (Dummy for now, later integrate APIs)
		public async Task<AnalyticsDTO> GetAnalyticsAsync()
		{
			var totalUsers = await _context.Users.CountAsync();

			return new AnalyticsDTO
			{
				TotalOrders = 0,
				TotalRevenue = 0,
				ActiveUsers = totalUsers
			};
		}

		//  Disputes
		public async Task<IEnumerable<Dispute>> GetAllDisputesAsync()
		{
			return await _context.Disputes.ToListAsync();
		}

		//  Refund
		public async Task<Refund> ProcessRefundAsync(RefundDTO refundDTO)
		{
			var refund = new Refund
			{
				OrderId = refundDTO.OrderId,
				Amount = refundDTO.Amount,
				Status = "Processed"
			};

			_context.Refunds.Add(refund);
			await _context.SaveChangesAsync();

			return refund;
		}

		//  Banner
		public async Task<Banner> CreateBannerAsync(BannerDTO bannerDTO)
		{
			var banner = new Banner
			{
				Title = bannerDTO.Title,
				ImageUrl = bannerDTO.ImageUrl,
				IsActive = true
			};

			_context.Banners.Add(banner);
			await _context.SaveChangesAsync();

			return banner;
		}

		public async Task<IEnumerable<Banner>> GetAllBannersAsync()
		{
			return await _context.Banners.ToListAsync();
		}

		//  Featured Restaurant
		public async Task<FeaturedRestaurant> AddFeaturedRestaurantAsync(int restaurantId)
		{
			var featured = new FeaturedRestaurant
			{
				RestaurantId = restaurantId
			};

			_context.FeaturedRestaurants.Add(featured);
			await _context.SaveChangesAsync();

			return featured;
		}

		//  CSV Export (basic)
		public async Task<byte[]> ExportOrdersReportAsync(DateTime startDate, DateTime endDate)
		{
			var data = await _context.Refunds
				.Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
				.ToListAsync();

			var csv = "OrderId,Amount,Status\n";

			foreach (var item in data)
			{
				csv += $"{item.OrderId},{item.Amount},{item.Status}\n";
			}

			return System.Text.Encoding.UTF8.GetBytes(csv);
		}

		public async Task<byte[]> ExportRevenueReportAsync(DateTime startDate, DateTime endDate)
		{
			var data = await _context.Refunds
				.Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
				.ToListAsync();

			var total = data.Sum(x => x.Amount);

			var csv = "TotalRevenue\n";
			csv += $"{total}";

			return System.Text.Encoding.UTF8.GetBytes(csv);
		}
	}
}
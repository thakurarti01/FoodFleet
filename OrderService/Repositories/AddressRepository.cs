using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories
{
	public class AddressRepository
	{
		private readonly OrderDbContext _context;

		public AddressRepository(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<List<DeliveryAddress>> GetByCustomerAsync(int customerId)
		{
			return await _context.DeliveryAddresses
				.Where(a => a.CustomerId == customerId)
				.ToListAsync();
		}

		public async Task<DeliveryAddress> AddAsync(DeliveryAddress address)
		{
			_context.DeliveryAddresses.Add(address);
			await _context.SaveChangesAsync();
			return address;
		}

		public async Task<DeliveryAddress> UpdateAsync(DeliveryAddress address)
		{
			_context.DeliveryAddresses.Update(address);
			await _context.SaveChangesAsync();
			return address;
		}

		public async Task<bool> DeleteAsync(int addressId)
		{
			var address = await _context.DeliveryAddresses.FindAsync(addressId);
			if (address == null) return false;

			_context.DeliveryAddresses.Remove(address);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
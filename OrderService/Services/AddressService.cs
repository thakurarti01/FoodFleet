using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Services
{
	public class AddressService : IAddressService
	{
		private readonly OrderDbContext _context;

		public AddressService(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<List<DeliveryAddress>> GetAddressesByCustomerAsync(int customerId)
		{
			return await _context.DeliveryAddresses
				.Where(a => a.CustomerId == customerId)
				.ToListAsync();
		}

		public async Task<DeliveryAddress> AddAddressAsync(AddressDTO addressDto)
		{
			var address = new DeliveryAddress
			{
				CustomerId = addressDto.CustomerId,
				AddressLine = addressDto.AddressLine,
				City = addressDto.City,
				State = addressDto.State,
				ZipCode = addressDto.ZipCode,
				IsDefault = addressDto.IsDefault
			};

			_context.DeliveryAddresses.Add(address);
			await _context.SaveChangesAsync();

			return address;
		}

		public async Task<DeliveryAddress> UpdateAddressAsync(int addressId, AddressDTO addressDto)
		{
			var address = await _context.DeliveryAddresses.FindAsync(addressId);
			if (address == null) return null;

			address.AddressLine = addressDto.AddressLine;
			address.City = addressDto.City;
			address.State = addressDto.State;
			address.ZipCode = addressDto.ZipCode;
			address.IsDefault = addressDto.IsDefault;

			await _context.SaveChangesAsync();
			return address;
		}

		public async Task<bool> DeleteAddressAsync(int addressId)
		{
			var address = await _context.DeliveryAddresses.FindAsync(addressId);
			if (address == null) return false;

			_context.DeliveryAddresses.Remove(address);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
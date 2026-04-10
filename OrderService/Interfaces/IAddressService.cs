using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
	public interface IAddressService
	{
		Task<List<DeliveryAddress>> GetAddressesByCustomerAsync(int customerId);
		Task<DeliveryAddress> AddAddressAsync(AddressDTO addressDto);
		Task<DeliveryAddress> UpdateAddressAsync(int addressId, AddressDTO addressDto);
		Task<bool> DeleteAddressAsync(int addressId);
	}
}
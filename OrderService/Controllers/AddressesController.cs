using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OrderService.DTOs;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AddressesController : ControllerBase
	{
		private readonly IAddressService _addressService;

		public AddressesController(IAddressService addressService)
		{
			_addressService = addressService;
		}

		[HttpGet("{customerId}")]
		public async Task<IActionResult> GetAddresses(int customerId)
		{
			var addresses = await _addressService.GetAddressesByCustomerAsync(customerId);
			return Ok(addresses);
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddAddress([FromBody] AddressDTO addressDto)
		{
			var address = await _addressService.AddAddressAsync(addressDto);
			return Ok(address);
		}

		[HttpPut("{addressId}")]
		public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] AddressDTO addressDto)
		{
			var address = await _addressService.UpdateAddressAsync(addressId, addressDto);
			return Ok(address);
		}

		[HttpDelete("{addressId}")]
		public async Task<IActionResult> DeleteAddress(int addressId)
		{
			var result = await _addressService.DeleteAddressAsync(addressId);
			if (!result) return NotFound();
			return Ok();
		}
	}
}
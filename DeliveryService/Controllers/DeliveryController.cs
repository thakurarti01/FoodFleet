using Microsoft.AspNetCore.Authorization;
using DeliveryService.DTOs;
using DeliveryService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers
{
    [ApiController]
    [Route("api/delivery")]
    public class DeliveryController : ControllerBase
	{
		private readonly IDeliveryService _deliveryService;

		public DeliveryController(IDeliveryService deliveryService)
		{
			_deliveryService = deliveryService;
		}

		//  Assign Delivery
		[HttpPost("assign")]
		public async Task<IActionResult> AssignDelivery(DeliveryDTO dto)
		{
			try
			{
				var delivery = await _deliveryService.AssignDeliveryAsync(dto);
				return Ok(delivery);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		//  Accept / Reject
		[HttpPut("{id}/response")]
		public async Task<IActionResult> Respond(int id, ResponseDTO dto)
		{
			var result = await _deliveryService.RespondToDeliveryAsync(id, dto.IsAccepted);
			if (!result) return NotFound();

			return Ok("Response recorded");
		}

		//  Update Status
		[HttpPut("{id}/status")]
		public async Task<IActionResult> UpdateStatus(int id, StatusUpdateDTO dto)
		{
			var result = await _deliveryService.UpdateStatusAsync(id, dto.Status);
			if (!result) return NotFound();

			return Ok("Status updated");
		}

		//  Get Delivery
		[HttpGet("{id}")]
		public async Task<IActionResult> GetDelivery(int id)
		{
			var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
			if (delivery == null) return NotFound();

			return Ok(delivery);
		}

		//  Track Delivery
		[HttpGet("{id}/track")]
		public async Task<IActionResult> Track(int id)
		{
			var data = await _deliveryService.TrackDeliveryAsync(id);
			if (data == null) return NotFound();

			return Ok(data);
		}
	}
}
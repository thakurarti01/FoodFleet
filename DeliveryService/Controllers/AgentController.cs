using DeliveryService.DTOs;
using DeliveryService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers
{
	[ApiController]
	[Route("api/agents")]
	public class AgentsController : ControllerBase
	{
		private readonly IAgentService _agentService;

		public AgentsController(IAgentService agentService)
		{
			_agentService = agentService;
		}

		//  Register Agent
		[HttpPost]
		public async Task<IActionResult> RegisterAgent(AgentDTO dto)
		{
			var agent = await _agentService.RegisterAgentAsync(dto);
			return Ok(agent);
		}

		//  Update Availability
		[HttpPut("{id}/availability")]
		public async Task<IActionResult> UpdateAvailability(int id, [FromQuery] bool isAvailable)
		{
			var result = await _agentService.UpdateAvailabilityAsync(id, isAvailable);
			if (!result) return NotFound();

			return Ok("Availability updated");
		}

		//  Update Location
		[HttpPut("{id}/location")]
		public async Task<IActionResult> UpdateLocation(int id, LocationDTO dto)
		{
			var result = await _agentService.UpdateLocationAsync(id, dto);
			if (!result) return NotFound();

			return Ok("Location updated");
		}

		//  Delivery History
		[HttpGet("{id}/history")]
		public async Task<IActionResult> GetHistory(int id)
		{
			var history = await _agentService.GetAgentHistoryAsync(id);
			return Ok(history);
		}
	}
}
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _orderService;

		public OrdersController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		// POST: api/orders
		[HttpPost]
		public async Task<IActionResult> PlaceOrder([FromBody] OrderDTO orderDTO)
		{
			var order = await _orderService.PlaceOrderAsync(orderDTO);
			return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, _orderService.MapToDTO(order));
		}

		// GET: api/orders/{orderId}
		[HttpGet("{orderId}")]
		public async Task<IActionResult> GetOrderById(int orderId)
		{
			var order = await _orderService.GetOrderByIdAsync(orderId);
			return Ok(_orderService.MapToDTO(order));
		}

		// GET: api/orders/customer/{customerId}
		[HttpGet("customer/{customerId}")]
		public async Task<IActionResult> GetOrdersByCustomer(int customerId)
		{
			var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
			var orderDTOs = orders.Select(o => _orderService.MapToDTO(o));
			return Ok(orderDTOs);
		}

		// PUT: api/orders/{orderId}/cancel
		[HttpPut("{orderId}/cancel")]
		public async Task<IActionResult> CancelOrder(int orderId)
		{
			var result = await _orderService.CancelOrderAsync(orderId);
			if (!result) return BadRequest("Cannot cancel this order.");
			return NoContent();
		}

		// PUT: api/orders/{orderId}/status
		[HttpPut("{orderId}/status")]
		public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] string newStatus)
		{
			var result = await _orderService.UpdateStatusAsync(orderId, newStatus);
			if (!result) return BadRequest("Cannot update status for this order.");
			return NoContent();
		}
	}
}
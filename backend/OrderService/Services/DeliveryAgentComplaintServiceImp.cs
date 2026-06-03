using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Interfaces;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// Manages complaints against delivery agents.
    /// Tracks complaint count and auto-suspends agent after 5 active complaints.
    /// </summary>
    public class DeliveryAgentComplaintServiceImp : IDeliveryAgentComplaintService
    {
        private readonly OrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeliveryAgentComplaintServiceImp(OrderDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<DeliveryAgentComplaint> CreateAsync(Guid deliveryAgentId, CreateDeliveryAgentComplaintDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.DeliveryAgentId != deliveryAgentId)
                throw new Exception("This order was not delivered by the specified agent");

            var complaint = new DeliveryAgentComplaint
            {
                DeliveryAgentId = deliveryAgentId,
                CustomerId = dto.CustomerId,
                OrderId = dto.OrderId,
                ComplaintType = dto.ComplaintType,
                Description = dto.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryAgentComplaints.Add(complaint);
            await _context.SaveChangesAsync();

            // Check if agent should be suspended (5 or more active complaints)
            var activeCount = await GetActiveComplaintCountAsync(deliveryAgentId);
            if (activeCount >= 5)
            {
                await SuspendDeliveryAgentAsync(deliveryAgentId);
                Console.WriteLine($"[ComplaintService] Delivery agent {deliveryAgentId} auto-suspended due to {activeCount} complaints.");
            }

            return complaint;
        }

        public async Task<IEnumerable<DeliveryAgentComplaint>> GetByAgentAsync(Guid deliveryAgentId)
        {
            return await _context.DeliveryAgentComplaints
                .Where(c => c.DeliveryAgentId == deliveryAgentId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryAgentComplaint>> GetAllAsync()
        {
            return await _context.DeliveryAgentComplaints
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<DeliveryAgentComplaint> ResolveAsync(int complaintId, ResolveDeliveryAgentComplaintDto dto)
        {
            var complaint = await _context.DeliveryAgentComplaints.FindAsync(complaintId);
            if (complaint == null)
                throw new Exception("Complaint not found");

            complaint.Status = dto.Status;
            complaint.AdminResponse = dto.AdminResponse;
            complaint.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<int> GetActiveComplaintCountAsync(Guid deliveryAgentId)
        {
            return await _context.DeliveryAgentComplaints
                .Where(c => c.DeliveryAgentId == deliveryAgentId && c.Status == "Pending" && !c.IsDeleted)
                .CountAsync();
        }

        private async Task SuspendDeliveryAgentAsync(Guid agentId)
        {
            try
            {
                var suspensionData = new
                {
                    suspendedUntil = DateTime.UtcNow.AddMonths(1),
                    suspensionReason = "Automatically suspended due to 5 or more unresolved complaints"
                };

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(suspensionData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync($"http://localhost:5213/api/users/{agentId}/suspend", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ComplaintService] Failed to suspend agent {agentId}: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"[ComplaintService] Agent {agentId} suspended until {suspensionData.suspendedUntil:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ComplaintService] Error suspending agent {agentId}: {ex.Message}");
            }
        }
    }
}

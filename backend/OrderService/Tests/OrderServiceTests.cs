using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Tests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private OrderDbContext _context = null!;
        private OrderServiceImp _service = null!;

        // ── Helpers ──────────────────────────────────────────────────────────

        private static OrderDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new OrderDbContext(options);
        }

        private static PlaceOrderDto SampleOrderDto(Guid? userId = null, Guid? restaurantId = null) => new()
        {
            UserId = userId ?? Guid.NewGuid(),
            RestaurantId = restaurantId ?? Guid.NewGuid(),
            DeliveryAddress = "123 Test Street, Mumbai",
            Items = new List<OrderItemDto>
            {
                new() { MenuItemId = 1, MenuItemName = "Butter Chicken", Price = 350m, Quantity = 2 },
                new() { MenuItemId = 2, MenuItemName = "Naan",           Price = 50m,  Quantity = 3 }
            }
        };

        [SetUp]
        public void SetUp()
        {
            // Each test gets a fresh in-memory DB
            _context = CreateInMemoryContext(Guid.NewGuid().ToString());
            _service = new OrderServiceImp(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ── PlaceOrderAsync ───────────────────────────────────────────────────

        [Test]
        public async Task PlaceOrder_ValidDto_ReturnsOrderWithCorrectTotalAmount()
        {
            var dto = SampleOrderDto();

            var order = await _service.PlaceOrderAsync(dto);

            // 350*2 + 50*3 = 700 + 150 = 850
            Assert.That(order.TotalAmount, Is.EqualTo(850m));
        }

        [Test]
        public async Task PlaceOrder_ValidDto_SetsStatusToPlaced()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            Assert.That(order.Status, Is.EqualTo("Placed"));
        }

        [Test]
        public async Task PlaceOrder_ValidDto_SetsDeliveryStatusToPending()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            Assert.That(order.DeliveryStatus, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task PlaceOrder_ValidDto_SetsPaymentStatusToPending()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            Assert.That(order.PaymentStatus, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task PlaceOrder_ValidDto_PersistsItemsToDatabase()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            var saved = await _context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id);
            Assert.That(saved.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task PlaceOrder_ValidDto_SnapshotsMenuItemName()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            Assert.That(order.Items[0].MenuItemName, Is.EqualTo("Butter Chicken"));
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Test]
        public async Task GetById_ExistingOrder_ReturnsOrderWithItems()
        {
            var placed = await _service.PlaceOrderAsync(SampleOrderDto());

            var fetched = await _service.GetByIdAsync(placed.Id);

            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetById_NonExistentId_ReturnsNull()
        {
            var result = await _service.GetByIdAsync(9999);

            Assert.That(result, Is.Null);
        }

        // ── GetByUserAsync ────────────────────────────────────────────────────

        [Test]
        public async Task GetByUser_ReturnsOnlyOrdersForThatUser()
        {
            var userId = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            await _service.PlaceOrderAsync(SampleOrderDto(userId: userId));
            await _service.PlaceOrderAsync(SampleOrderDto(userId: userId));
            await _service.PlaceOrderAsync(SampleOrderDto(userId: otherId));

            var orders = await _service.GetByUserAsync(userId);

            Assert.That(orders.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByUser_NoOrders_ReturnsEmptyList()
        {
            var orders = await _service.GetByUserAsync(Guid.NewGuid());

            Assert.That(orders, Is.Empty);
        }

        // ── GetByRestaurantAsync ──────────────────────────────────────────────

        [Test]
        public async Task GetByRestaurant_ReturnsOnlyOrdersForThatRestaurant()
        {
            var restaurantId = Guid.NewGuid();

            await _service.PlaceOrderAsync(SampleOrderDto(restaurantId: restaurantId));
            await _service.PlaceOrderAsync(SampleOrderDto(restaurantId: restaurantId));
            await _service.PlaceOrderAsync(SampleOrderDto()); // different restaurant

            var orders = await _service.GetByRestaurantAsync(restaurantId);

            Assert.That(orders.Count(), Is.EqualTo(2));
        }

        // ── UpdateStatusAsync ─────────────────────────────────────────────────

        [Test]
        public async Task UpdateStatus_ValidOrder_UpdatesStatusAndReturnsTrue()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            var result = await _service.UpdateStatusAsync(order.Id, "Confirmed");

            Assert.That(result, Is.True);
            var updated = await _context.Orders.FindAsync(order.Id);
            Assert.That(updated!.Status, Is.EqualTo("Confirmed"));
        }

        [Test]
        public async Task UpdateStatus_CancelledOrder_ReturnsFalse()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());
            await _service.CancelAsync(order.Id, "Changed mind");

            var result = await _service.UpdateStatusAsync(order.Id, "Confirmed");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdateStatus_NonExistentOrder_ReturnsFalse()
        {
            var result = await _service.UpdateStatusAsync(9999, "Confirmed");

            Assert.That(result, Is.False);
        }

        // ── CancelAsync ───────────────────────────────────────────────────────

        [Test]
        public async Task Cancel_PlacedOrder_SetsCancelledStatusAndReason()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            var result = await _service.CancelAsync(order.Id, "Ordered by mistake");

            Assert.That(result, Is.True);
            var updated = await _context.Orders.FindAsync(order.Id);
            Assert.That(updated!.Status, Is.EqualTo("Cancelled"));
            Assert.That(updated.CancellationReason, Is.EqualTo("Ordered by mistake"));
        }

        [Test]
        public async Task Cancel_AlreadyCancelledOrder_ReturnsFalse()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());
            await _service.CancelAsync(order.Id, "First cancel");

            var result = await _service.CancelAsync(order.Id, "Second cancel");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Cancel_DeliveredOrder_ReturnsFalse()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());
            await _service.UpdateDeliveryStatusAsync(order.Id, "Delivered");

            var result = await _service.CancelAsync(order.Id, "Too late");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Cancel_NonExistentOrder_ReturnsFalse()
        {
            var result = await _service.CancelAsync(9999, "reason");

            Assert.That(result, Is.False);
        }

        // ── AssignAgentAsync ──────────────────────────────────────────────────

        [Test]
        public async Task AssignAgent_ValidOrder_SetsAgentIdAndDeliveryStatusAssigned()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());
            var agentId = Guid.NewGuid();

            var result = await _service.AssignAgentAsync(order.Id, agentId);

            Assert.That(result, Is.True);
            var updated = await _context.Orders.FindAsync(order.Id);
            Assert.That(updated!.DeliveryAgentId, Is.EqualTo(agentId));
            Assert.That(updated.DeliveryStatus, Is.EqualTo("Assigned"));
        }

        [Test]
        public async Task AssignAgent_NonExistentOrder_ReturnsFalse()
        {
            var result = await _service.AssignAgentAsync(9999, Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        // ── UpdateDeliveryStatusAsync ─────────────────────────────────────────

        [Test]
        public async Task UpdateDeliveryStatus_PickedUp_UpdatesDeliveryStatusOnly()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            var result = await _service.UpdateDeliveryStatusAsync(order.Id, "PickedUp");

            Assert.That(result, Is.True);
            var updated = await _context.Orders.FindAsync(order.Id);
            Assert.That(updated!.DeliveryStatus, Is.EqualTo("PickedUp"));
            Assert.That(updated.Status, Is.EqualTo("Placed")); // order status unchanged
        }

        [Test]
        public async Task UpdateDeliveryStatus_Delivered_SetsOrderStatusAndPaymentStatusToPaid()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());

            await _service.UpdateDeliveryStatusAsync(order.Id, "Delivered");

            var updated = await _context.Orders.FindAsync(order.Id);
            Assert.That(updated!.Status, Is.EqualTo("Delivered"));
            Assert.That(updated.PaymentStatus, Is.EqualTo("Paid"));
            Assert.That(updated.DeliveryStatus, Is.EqualTo("Delivered"));
        }

        [Test]
        public async Task UpdateDeliveryStatus_NonExistentOrder_ReturnsFalse()
        {
            var result = await _service.UpdateDeliveryStatusAsync(9999, "PickedUp");

            Assert.That(result, Is.False);
        }
    }
}

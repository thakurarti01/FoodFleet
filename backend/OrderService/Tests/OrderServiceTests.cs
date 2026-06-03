using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Services;
using Moq;
using System.Net.Http;

namespace OrderService.Tests
{
    [TestFixture] //makes a test class in NUnit
    public class OrderServiceTests
    {
        private OrderDbContext _context = null!;
        private OrderServiceImp _service = null!;

        // ── Helpers ──────────────────────────────────────────────────────────

        //creating fake in-memory database context for testing, so that we can test the service without needing a real database
        //dbName - name of fake db
        private static OrderDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase(dbName) //thissays do not use real db, instead use in-memory db with this name
                .Options;
            return new OrderDbContext(options); //creating actual db context obj but is not connected to real db
        }

        // creating a sample order dto for testing, with default values for userId and restaurantId, but can be overridden if needed
        private static PlaceOrderDto SampleOrderDto(Guid? userId = null, Guid? restaurantId = null) => new()
        {
            UserId = userId ?? Guid.NewGuid(), // if userid is provided-use it, otherwise generate a new random userid
            RestaurantId = restaurantId ?? Guid.NewGuid(), //use given restaurantId OR generate new one
            DeliveryAddress = "123 Test Street, Mumbai", // dummy address  
            Items = new List<OrderItemDto>
            {
                new() { MenuItemId = 1, MenuItemName = "Butter Chicken", Price = 350m, Quantity = 2 },
                new() { MenuItemId = 2, MenuItemName = "Naan",           Price = 50m,  Quantity = 3 }
            }
        };

        [SetUp] // runs before each test method
        public void SetUp()
        {
            // Each test gets a fresh in-memory DB
            // no data sharing between tests, and no need to clean up
            _context = CreateInMemoryContext(Guid.NewGuid().ToString());

            // RabbitMQPublisher gracefully handles unavailable RabbitMQ (_available = false)
            // if rabbitmq is not running, it will not crash, will silently disables messaging
            var publisher = new RabbitMQPublisher();

            // Mock IHttpClientFactory — no real HTTP calls needed in tests
            // in unit tests we NEVER call real API endpoints, so we are using moq framework
            var mockHttpFactory = new Mock<IHttpClientFactory>();
            mockHttpFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            // creating the real service class manually inside the test
            _service = new OrderServiceImp(_context, publisher, mockHttpFactory.Object);
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
            //arrange - create input
            var dto = SampleOrderDto();
            //act - call service method
            var order = await _service.PlaceOrderAsync(dto);

            // assert - 350*2 + 50*3 = 700 + 150 = 850
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

            Assert.ThrowsAsync<OrderService.Exceptions.OrderAlreadyCancelledException>(
                () => _service.UpdateStatusAsync(order.Id, "Confirmed"));
        }

        [Test]
        public void UpdateStatus_NonExistentOrder_ReturnsFalse()
        {
            Assert.ThrowsAsync<OrderService.Exceptions.OrderNotFoundException>(
                () => _service.UpdateStatusAsync(9999, "Confirmed"));
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

            Assert.ThrowsAsync<OrderService.Exceptions.OrderAlreadyCancelledException>(
                () => _service.CancelAsync(order.Id, "Second cancel"));
        }

        [Test]
        public async Task Cancel_DeliveredOrder_ReturnsFalse()
        {
            var order = await _service.PlaceOrderAsync(SampleOrderDto());
            await _service.UpdateDeliveryStatusAsync(order.Id, "Delivered");

            Assert.ThrowsAsync<OrderService.Exceptions.OrderNotCancellableException>(
                () => _service.CancelAsync(order.Id, "Too late"));
        }

        [Test]
        public void Cancel_NonExistentOrder_ReturnsFalse()
        {
            //expecting this async method to throw a specific exception when we try to cancel an order that does not exist in the database, and we are asserting that the exception is thrown as expected
            Assert.ThrowsAsync<OrderService.Exceptions.OrderNotFoundException>(
                () => _service.CancelAsync(9999, "reason"));
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
        public void AssignAgent_NonExistentOrder_ReturnsFalse()
        {
            Assert.ThrowsAsync<OrderService.Exceptions.OrderNotFoundException>(
                () => _service.AssignAgentAsync(9999, Guid.NewGuid()));
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
        public void UpdateDeliveryStatus_NonExistentOrder_ReturnsFalse()
        {
            Assert.ThrowsAsync<OrderService.Exceptions.OrderNotFoundException>(
                () => _service.UpdateDeliveryStatusAsync(9999, "PickedUp"));
        }
    }
}

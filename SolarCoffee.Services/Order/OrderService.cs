using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarCoffee.Data;
using SolarCoffee.Data.Models;
using SolarCoffee.Services.Inventory;
using SolarCoffee.Services.Product;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SolarCoffee.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly SolarDbContext _db;
        private readonly ILogger<OrderService> _logger;
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;

        public OrderService(SolarDbContext dbContext, 
            ILogger<OrderService> logger,
            IProductService productService, 
            IInventoryService inventoryService)
        {
            _db = dbContext;
            _logger = logger;
            _productService = productService;
            _inventoryService = inventoryService;
        }
        
        public ServiceResponse<bool> GenerateOpenrOrder(SalesOrder order)
        {

            _logger.LogInformation($"Generating new order");

            foreach(var item in order.SalesOrderItems)
            {
                item.Product = _productService
                    .GetProductById(item.Product.Id);
 
                var inventoryId = _inventoryService
                    .GetByProductId(item.Product.Id).Id;

                _inventoryService.UpdateUnitsAvailable(inventoryId, -item.Quantity);

            }
            try
            {
                _db.SalesOrders.Add(order);
                _db.SaveChanges();

                return new ServiceResponse<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = "Open order created",
                    Time = DateTime.UtcNow
                };
            }
            catch (Exception e)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = e.StackTrace,
                    Time = DateTime.UtcNow
                };
            }
        }

        public List<SalesOrder> GetOrders()
        {
            return _db.SalesOrders
                .Include(ords => ords.Customer)
                    .ThenInclude(customer => customer.PrimaryAddress)
                .Include(ords => ords.SalesOrderItems)
                    .ThenInclude(item => item.Product)
                .ToList();
        }

        public ServiceResponse<bool> MarkFulfilled(int id)
        {
            var now = DateTime.UtcNow;
            var order = _db.SalesOrders.Find(id);
            order.UpdatedOn = now;
            order.IsPaid = true;

            try
            {
                _db.SalesOrders.Update(order);
                _db.SaveChanges();

                return new ServiceResponse<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = $"Order {order.Id} closed, invoice paid in full",
                    Time = now
                };
            } catch (Exception e)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = e.StackTrace,
                    Time = now
                };
            }

        }
    }
}

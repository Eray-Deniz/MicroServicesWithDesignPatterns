using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public readonly AppDbContext _context;

        public readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Adress = new Address { Line = orderCreate.Adress.Line, District = orderCreate.Adress.District, Province = orderCreate.Adress.Province },
                CreatedDate = DateTime.Now
            };

            orderCreate.orderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem()
                {
                    Price = item.Price,
                    ProductId = item.ProductId,
                    Count = item.Count
                });
            }
            );

            await _context.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvemt()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage()
                {
                    CardName = orderCreate.payment.CardName,
                    CardNumber = orderCreate.payment.CardNumber,
                    Expiration = orderCreate.payment.Expiration,
                    CVV = orderCreate.payment.CVV,
                    TotalPrice = orderCreate.orderItems.Sum(item => item.Price * item.Count)
                }
            };

            orderCreate.orderItems.ForEach(item =>
            {
                orderCreatedEvent.orderItems.Add(new OrderItemMessage()
                {
                    Count = item.Count,
                    ProductId = item.ProductId,
                });
            }
            );

            /*
            Publish ile gönderilen mesajda RabitMQ Exchange e mesaj gönderir, kimlerin dinlediği bilinmez, exchange e subsucribe olmuş kuyruk yoksa(dinleyen yoksa) mesaj boşa gider.
            Send ile gönderilen mesajlar direk olarak kuyruğa gider. Genellikle tek bir microservis(payment gibi) dinliyorsa send ile gönderilir.
            Gönderilen event i birden fazla servis dinleyecekse publish, tek bir servis dinleyecekse send ile gönderilir.
            */
            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
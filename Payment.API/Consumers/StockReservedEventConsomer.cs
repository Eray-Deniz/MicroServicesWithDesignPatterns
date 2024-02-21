using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsomer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsomer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsomer(ILogger<StockReservedEventConsomer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;//m=> decimal

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was widhdrawn from credit card for user id: {context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentCompletedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId });
            }
            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not widhdrawn from credit card for user id: {context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymenFailedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId, Message = "not enough balance", OrderItems = context.Message.OrderItems });
            }
        }
    }
}
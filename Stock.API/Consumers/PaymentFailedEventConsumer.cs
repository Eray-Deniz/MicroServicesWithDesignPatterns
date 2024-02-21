using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Stock.API.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymenFailedEvent>
    {
        private readonly AppDbContext _context;
        private ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymenFailedEvent> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;

                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation($"Stock was released for Buyer Id {context.Message.OrderId}");
        }
    }
}
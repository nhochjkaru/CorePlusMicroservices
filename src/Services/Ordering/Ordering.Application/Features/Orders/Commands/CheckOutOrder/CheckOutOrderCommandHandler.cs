using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands.CheckOutOrder
{
    public class CheckOutOrderCommandHandler : IRequestHandler<CheckOutOrderCommand, int>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailservice;
        private readonly ILogger<CheckOutOrderCommandHandler> _logging;

        public CheckOutOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailservice, ILogger<CheckOutOrderCommandHandler> logging)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailservice = emailservice ?? throw new ArgumentNullException(nameof(emailservice));
            _logging = logging ?? throw new ArgumentNullException(nameof(logging));
        }

        public async Task<int> Handle(CheckOutOrderCommand request, CancellationToken cancellationToken)
        {
            var OrderEntity = _mapper.Map<Order>(request);
            var newOrder = await _orderRepository.AddAsync(OrderEntity);

            _logging.LogInformation($"Order {newOrder.Id} sucessful");
            await SendMail(newOrder);
            return newOrder.Id;
        }
        private async Task SendMail(Order order)
        {
            var email = new Email() { To = "shinehjkaru@gmail.com", Body = $"Order was created.", Subject = "Order was created" };

            try
            {
                await _emailservice.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logging.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
            }
        }
    }
}

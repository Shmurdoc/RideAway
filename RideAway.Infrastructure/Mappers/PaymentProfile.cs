using AutoMapper;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Infrastructure.Mappers
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // From CreatePaymentRequest to Payment Entity
            CreateMap<CreatePaymentRequestDTO, Payment>();

            // From Payment Entity to DTO
            CreateMap<Payment, PaymentDTO>();

           
        }
    }
}

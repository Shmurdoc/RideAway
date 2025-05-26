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

    public class RideProfile : Profile
    {
        public RideProfile()
        {
            // Mapping CreateRideRequestDTO to Ride
            CreateMap<CreateRideRequestDTO, Ride>()
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId))
                .ForMember(dest => dest.PickupLocation, opt => opt.MapFrom(src => src.PickupLocation))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.Fare, opt => opt.MapFrom(src => src.EstimatedFare)) // EstimatedFare -> Fare
                .ForMember(dest => dest.RiderCategory, opt => opt.MapFrom(src => src.RideCategory))
                .ForMember(dest => dest.Rider, opt => opt.Ignore()) // Rider is a navigation property
                .ForMember(dest => dest.Driver, opt => opt.Ignore()) // Driver is a navigation property
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // Status defaults to Requested

            // Mapping Ride to RideDTO
            CreateMap<Ride, RideDTO>()
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId))
                .ForMember(dest => dest.PickupLocation, opt => opt.MapFrom(src => src.PickupLocation))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.EstimatedFare, opt => opt.MapFrom(src => src.Fare)) // Fare -> EstimatedFare
                .ForMember(dest => dest.RideCategory, opt => opt.MapFrom(src => src.RiderCategory))
                .ForMember(dest => dest.RiderId, opt => opt.MapFrom(src => src.RiderId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)); 
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Infrastructure.Mappers
{
    using AutoMapper;
    using RideAway.Application.DTOs;
    using RideAway.Domain.Entities;

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapping CreateUserDTO to User
            CreateMap<CreateUserDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentLocation, opt => opt.Ignore());

            // Mapping User to UserDTO
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // Mapping User to DriverLocationUpdateDTO
            CreateMap<User, DriverLocationUpdateDTO>()
                .ForMember(dest => dest.CurrentLocation, opt => opt.MapFrom(src => src.CurrentLocation))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}

using Application.Features.Branchs.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class BranchProfile : Profile
    {
        public BranchProfile()
        {
            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Country))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.Address.District))
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Address.Neighborhood))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
                .ForMember(dest => dest.ApartmentNumber, opt => opt.MapFrom(src => src.Address.ApartmentNumber))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Address.ZipCode))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Address.Location));
        }
    }
}

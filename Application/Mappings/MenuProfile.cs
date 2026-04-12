using Application.Features.Menus.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            // MenuItem Mapping
            CreateMap<MenuItem, MenuItemDto>()
                .ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src.CategoryType.ToString()));

            // Menu Mapping
            CreateMap<Menu, MenuDto>()
                .ForMember(dest => dest.MenuType, opt => opt.MapFrom(src => src.MenuType.ToString()));
            // MenuItems listesi AutoMapper tarafından otomatik eşleşir (isimler aynı olduğu için)

            CreateMap<Branch, BranchMenusGroupDto>()
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
                .ForMember(dest => dest.Menus, opt => opt.Ignore());
        }
    }
}

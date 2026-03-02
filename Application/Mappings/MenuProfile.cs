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
        }
    }
}

using Application.Features.Orders.Queries.Shared.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Query'ler için Entity -> DTO mapping
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.ShippingAddress.Street))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.ShippingAddress.City))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ShippingAddress.ZipCode));

            CreateMap<OrderItem, OrderItemDto>(); // Zaten isimler aynı, otomatik eşleşir

            // Command'lar için DTO -> Entity mapping (Genelde pek sevilmez, manuel yapılması önerilir)
            // Biz de Handler'da manuel (new Address(...), Order.Create(...)) yaptık.
        }
    }
}

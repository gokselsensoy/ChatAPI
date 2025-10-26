using Application.Features.Brands.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            // Entity -> DTO mapping
            CreateMap<Brand, BrandDto>();

            // DTO -> Entity mapping'i AutoMapper ile yapmıyoruz.
            // Domain kurallarını (Fabrika Metotları) kullanıyoruz.
        }
    }
}

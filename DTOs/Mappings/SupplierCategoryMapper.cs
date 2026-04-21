using AutoMapper;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Responses.Supplier;

namespace BackEnd.Infrastructure.Mappings;

public class SupplierCategoryMapper : Profile
{
    public SupplierCategoryMapper()
    {
        
        CreateMap<SupplierCategory, SupplierCategoryResponseDto>();
        CreateMap<SupplierCategoryRequestDto, SupplierCategory>();
    }
}
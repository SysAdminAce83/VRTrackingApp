using AutoMapper;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Profiles
{
    public class ReferenceProfile : Profile
    {
        public ReferenceProfile()
        {
            CreateMap<Reference, ReferenceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VulnerabilityId, opt => opt.MapFrom(src => src.VulnerabilityId))
                .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src => src.ReferenceType))
                .ForMember(dest => dest.ReferenceValue, opt => opt.MapFrom(src => src.ReferenceValue))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        }
    }
}
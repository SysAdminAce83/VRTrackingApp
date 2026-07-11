using AutoMapper;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Profiles
{
    public class ScanProfile : Profile
    {
        public ScanProfile()
        {
            CreateMap<Scan, ScanDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ScanName, opt => opt.MapFrom(src => src.ScanName))
                .ForMember(dest => dest.ScanDate, opt => opt.MapFrom(src => src.ScanDate))
                .ForMember(dest => dest.ScanType, opt => opt.MapFrom(src => src.ScanType))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.FileHash, opt => opt.MapFrom(src => src.FileHash))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        }
    }
}
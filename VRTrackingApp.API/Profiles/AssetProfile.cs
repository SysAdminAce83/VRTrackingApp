using AutoMapper;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Profiles
{
    public class AssetProfile : Profile
    {
        public AssetProfile()
        {
            CreateMap<Asset, AssetDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ScanId, opt => opt.MapFrom(src => src.ScanId))
                .ForMember(dest => dest.HostName, opt => opt.MapFrom(src => src.HostName))
                .ForMember(dest => dest.IPAddress, opt => opt.MapFrom(src => src.IPAddress))
                .ForMember(dest => dest.MACAddress, opt => opt.MapFrom(src => src.MACAddress))
                .ForMember(dest => dest.DNSName, opt => opt.MapFrom(src => src.DNSName))
                .ForMember(dest => dest.OperatingSystem, opt => opt.MapFrom(src => src.OperatingSystem))
                .ForMember(dest => dest.OSVersion, opt => opt.MapFrom(src => src.OSVersion))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        }
    }
}
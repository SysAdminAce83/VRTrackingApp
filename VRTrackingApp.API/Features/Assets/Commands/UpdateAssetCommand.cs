using System;
using MediatR;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Features.Assets.Commands
{
    public class UpdateAssetCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int ScanId { get; set; }
        public string? HostName { get; set; }
        public string IPAddress { get; set; } = default!;
        public string? MACAddress { get; set; }
        public string? DNSName { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OSVersion { get; set; }
    }
}
using MediatR;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Features.Scans.Commands
{
    public record UpdateScanCommand : IRequest
    {
        public int Id { get; init; }
        public string ScanName { get; init; } = default!;
        public DateTime ScanDate { get; init; }
        public string ScanType { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public long? FileSize { get; init; }
        public string? FileHash { get; init; }
        public string Status { get; init; } = "Processing";

        public UpdateScanCommand()
        {
        }
    }
}
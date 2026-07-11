using MediatR;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Features.Scans.Queries
{
    public record GetScanByIdQuery : IRequest<ScanDto>
    {
        public int Id { get; init; }

        public GetScanByIdQuery(int id)
        {
            Id = id;
        }
    }
}
using MediatR;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Features.Scans.Queries
{
    public record GetScansQuery : IRequest<IReadOnlyList<ScanDto>>;
}
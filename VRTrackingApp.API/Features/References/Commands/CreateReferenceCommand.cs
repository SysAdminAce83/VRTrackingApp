using MediatR;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class CreateReferenceCommand : IRequest<ReferenceDto>
    {
        public int VulnerabilityId { get; set; }
        public string ReferenceType { get; set; } = default!;
        public string ReferenceValue { get; set; } = default!;
        public string? URL { get; set; }
    }
}
using MediatR;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class UpdateReferenceCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int VulnerabilityId { get; set; }
        public string ReferenceType { get; set; } = default!;
        public string ReferenceValue { get; set; } = default!;
        public string? URL { get; set; }
    }
}
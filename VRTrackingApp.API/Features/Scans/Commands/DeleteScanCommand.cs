using MediatR;

namespace VRTrackingApp.API.Features.Scans.Commands
{
    public record DeleteScanCommand : IRequest
    {
        public int Id { get; init; }

        public DeleteScanCommand(int id)
        {
            Id = id;
        }
    }
}
using MediatR;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class DeleteReferenceCommand : IRequest<Unit>
    {
        public int Id { get; }

        public DeleteReferenceCommand(int id)
        {
            Id = id;
        }
    }
}
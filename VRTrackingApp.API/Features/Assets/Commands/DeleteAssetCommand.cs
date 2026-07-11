using MediatR;

namespace VRTrackingApp.API.Features.Assets.Commands
{
    public class DeleteAssetCommand : IRequest<Unit>
    {
        public int Id { get; }

        public DeleteAssetCommand(int id)
        {
            Id = id;
        }
    }
}
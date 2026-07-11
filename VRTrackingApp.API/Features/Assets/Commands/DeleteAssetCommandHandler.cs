using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.API.Features.Assets.Commands
{
    public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, Unit>
    {
        private readonly IAssetRepository _assetRepository;

        public DeleteAssetCommandHandler(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<Unit> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null)
            {
                throw new KeyNotFoundException($"Asset with id {request.Id} not found");
            }

            await _assetRepository.DeleteAsync(asset);
            return Unit.Value;
        }
    }
}
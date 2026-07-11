using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Commands
{
    public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, Unit>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public UpdateAssetCommandHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null)
            {
                throw new KeyNotFoundException($"Asset with id {request.Id} not found");
            }

            asset.ScanId = request.ScanId;
            asset.HostName = request.HostName;
            asset.IPAddress = request.IPAddress;
            asset.MACAddress = request.MACAddress;
            asset.DNSName = request.DNSName;
            asset.OperatingSystem = request.OperatingSystem;
            asset.OSVersion = request.OSVersion;

            await _assetRepository.UpdateAsync(asset);
            return Unit.Value;
        }
    }
}
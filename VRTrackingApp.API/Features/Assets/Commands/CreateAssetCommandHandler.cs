using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Commands
{
    public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public CreateAssetCommandHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = new Asset
            {
                ScanId = request.ScanId,
                HostName = request.HostName,
                IPAddress = request.IPAddress,
                MACAddress = request.MACAddress,
                DNSName = request.DNSName,
                OperatingSystem = request.OperatingSystem,
                OSVersion = request.OSVersion
            };

            await _assetRepository.AddAsync(asset);
            return _mapper.Map<AssetDto>(asset);
        }
    }
}
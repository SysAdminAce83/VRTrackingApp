using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetByIpAddressQueryHandler : IRequestHandler<GetAssetByIpAddressQuery, AssetDto>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public GetAssetByIpAddressQueryHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<AssetDto> Handle(GetAssetByIpAddressQuery request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIpAddressAsync(request.IpAddress);
            return _mapper.Map<AssetDto>(asset);
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, IReadOnlyList<AssetDto>>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public GetAssetsQueryHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
        {
            var assets = await _assetRepository.ListAllAsync();
            return _mapper.Map<IReadOnlyList<AssetDto>>(assets);
        }
    }
}
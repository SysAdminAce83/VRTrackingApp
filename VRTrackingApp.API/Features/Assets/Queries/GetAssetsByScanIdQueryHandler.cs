using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsByScanIdQueryHandler : IRequestHandler<GetAssetsByScanIdQuery, IReadOnlyList<AssetDto>>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public GetAssetsByScanIdQueryHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsByScanIdQuery request, CancellationToken cancellationToken)
        {
            var assets = await _assetRepository.GetByScanIdAsync(request.ScanId);
            return _mapper.Map<IReadOnlyList<AssetDto>>(assets);
        }
    }
}
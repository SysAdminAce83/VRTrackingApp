using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.AssetVulnerabilities.Queries
{
    public class GetAssetVulnerabilitiesByAssetIdQueryHandler : IRequestHandler<GetAssetVulnerabilitiesByAssetIdQuery, IReadOnlyList<AssetVulnerabilityDto>>
    {
        private readonly IAssetVulnerabilityRepository _assetVulnerabilityRepository;
        private readonly IMapper _mapper;

        public GetAssetVulnerabilitiesByAssetIdQueryHandler(IAssetVulnerabilityRepository assetVulnerabilityRepository, IMapper mapper)
        {
            _assetVulnerabilityRepository = assetVulnerabilityRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AssetVulnerabilityDto>> Handle(GetAssetVulnerabilitiesByAssetIdQuery request, CancellationToken cancellationToken)
        {
            var assetVulnerabilities = await _assetVulnerabilityRepository.GetByAssetIdAsync(request.AssetId);
            return _mapper.Map<IReadOnlyList<AssetVulnerabilityDto>>(assetVulnerabilities);
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.AssetVulnerabilities.Queries
{
    public class GetAssetVulnerabilitiesQueryHandler : IRequestHandler<GetAssetVulnerabilitiesQuery, IReadOnlyList<AssetVulnerabilityDto>>
    {
        private readonly IAssetVulnerabilityRepository _assetVulnerabilityRepository;
        private readonly IMapper _mapper;

        public GetAssetVulnerabilitiesQueryHandler(IAssetVulnerabilityRepository assetVulnerabilityRepository, IMapper mapper)
        {
            _assetVulnerabilityRepository = assetVulnerabilityRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AssetVulnerabilityDto>> Handle(GetAssetVulnerabilitiesQuery request, CancellationToken cancellationToken)
        {
            var assetVulnerabilities = await _assetVulnerabilityRepository.ListAllAsync();
            return _mapper.Map<IReadOnlyList<AssetVulnerabilityDto>>(assetVulnerabilities);
        }
    }
}
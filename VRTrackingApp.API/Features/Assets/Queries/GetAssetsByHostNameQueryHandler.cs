using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsByHostNameQueryHandler : IRequestHandler<GetAssetsByHostNameQuery, IReadOnlyList<AssetDto>>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public GetAssetsByHostNameQueryHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsByHostNameQuery request, CancellationToken cancellationToken)
        {
            var assets = await _assetRepository.GetByHostNameAsync(request.HostName);
            return _mapper.Map<IReadOnlyList<AssetDto>>(assets);
        }
    }
}
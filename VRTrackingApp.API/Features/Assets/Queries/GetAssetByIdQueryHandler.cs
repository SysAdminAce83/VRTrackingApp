using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto>
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public GetAssetByIdQueryHandler(IAssetRepository assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<AssetDto> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            return _mapper.Map<AssetDto>(asset);
        }
    }
}
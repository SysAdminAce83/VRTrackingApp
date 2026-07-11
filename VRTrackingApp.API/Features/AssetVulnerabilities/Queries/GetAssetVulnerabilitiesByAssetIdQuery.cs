using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.AssetVulnerabilities.Queries
{
    public class GetAssetVulnerabilitiesByAssetIdQuery : IRequest<IReadOnlyList<AssetVulnerabilityDto>>
    {
        public int AssetId { get; }

        public GetAssetVulnerabilitiesByAssetIdQuery(int assetId)
        {
            AssetId = assetId;
        }
    }
}
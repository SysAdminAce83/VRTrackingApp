using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsByScanIdQuery : IRequest<IReadOnlyList<AssetDto>>
    {
        public int ScanId { get; }

        public GetAssetsByScanIdQuery(int scanId)
        {
            ScanId = scanId;
        }
    }
}
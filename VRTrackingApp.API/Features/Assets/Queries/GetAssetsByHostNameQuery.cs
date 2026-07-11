using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsByHostNameQuery : IRequest<IReadOnlyList<AssetDto>>
    {
        public string HostName { get; }

        public GetAssetsByHostNameQuery(string hostName)
        {
            HostName = hostName;
        }
    }
}
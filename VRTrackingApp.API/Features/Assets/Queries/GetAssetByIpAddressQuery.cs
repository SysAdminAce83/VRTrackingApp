using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetByIpAddressQuery : IRequest<AssetDto>
    {
        public string IpAddress { get; }

        public GetAssetByIpAddressQuery(string ipAddress)
        {
            IpAddress = ipAddress;
        }
    }
}
using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetsQuery : IRequest<IReadOnlyList<AssetDto>>
    {
    }
}
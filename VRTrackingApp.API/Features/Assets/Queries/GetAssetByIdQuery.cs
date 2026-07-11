using System;
using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Assets.Queries
{
    public class GetAssetByIdQuery : IRequest<AssetDto>
    {
        public int Id { get; }

        public GetAssetByIdQuery(int id)
        {
            Id = id;
        }
    }
}
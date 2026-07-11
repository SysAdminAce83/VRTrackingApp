using MediatR;
using System.Collections.Generic;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.AssetVulnerabilities.Queries
{
    public class GetAssetVulnerabilitiesQuery : IRequest<IReadOnlyList<AssetVulnerabilityDto>>
    {
    }
}
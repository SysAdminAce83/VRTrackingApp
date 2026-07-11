using MediatR;
using System.Collections.Generic;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Vulnerabilities.Queries
{
    public class GetVulnerabilitiesQuery : IRequest<IReadOnlyList<VulnerabilityDto>>
    {
    }
}
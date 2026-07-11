using MediatR;
using System.Collections.Generic;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.Vulnerabilities.Queries
{
    public class GetVulnerabilitiesBySeverityQuery : IRequest<IReadOnlyList<VulnerabilityDto>>
    {
        public string Severity { get; }

        public GetVulnerabilitiesBySeverityQuery(string severity)
        {
            Severity = severity;
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Vulnerabilities.Queries
{
    public class GetVulnerabilitiesBySeverityQueryHandler : IRequestHandler<GetVulnerabilitiesBySeverityQuery, IReadOnlyList<VulnerabilityDto>>
    {
        private readonly IVulnerabilityRepository _vulnerabilityRepository;
        private readonly IMapper _mapper;

        public GetVulnerabilitiesBySeverityQueryHandler(IVulnerabilityRepository vulnerabilityRepository, IMapper mapper)
        {
            _vulnerabilityRepository = vulnerabilityRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<VulnerabilityDto>> Handle(GetVulnerabilitiesBySeverityQuery request, CancellationToken cancellationToken)
        {
            var vulnerabilities = await _vulnerabilityRepository.GetBySeverityAsync(request.Severity);
            return _mapper.Map<IReadOnlyList<VulnerabilityDto>>(vulnerabilities);
        }
    }
}
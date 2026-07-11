using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Vulnerabilities.Queries
{
    public class GetVulnerabilitiesQueryHandler : IRequestHandler<GetVulnerabilitiesQuery, IReadOnlyList<VulnerabilityDto>>
    {
        private readonly IVulnerabilityRepository _vulnerabilityRepository;
        private readonly IMapper _mapper;

        public GetVulnerabilitiesQueryHandler(IVulnerabilityRepository vulnerabilityRepository, IMapper mapper)
        {
            _vulnerabilityRepository = vulnerabilityRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<VulnerabilityDto>> Handle(GetVulnerabilitiesQuery request, CancellationToken cancellationToken)
        {
            var vulnerabilities = await _vulnerabilityRepository.ListAllAsync();
            return _mapper.Map<IReadOnlyList<VulnerabilityDto>>(vulnerabilities);
        }
    }
}
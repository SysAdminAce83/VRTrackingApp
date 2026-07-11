using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.References.Queries
{
    public class GetReferencesQueryHandler : IRequestHandler<GetReferencesQuery, IReadOnlyList<ReferenceDto>>
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IMapper _mapper;

        public GetReferencesQueryHandler(IReferenceRepository referenceRepository, IMapper mapper)
        {
            _referenceRepository = referenceRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ReferenceDto>> Handle(GetReferencesQuery request, CancellationToken cancellationToken)
        {
            var references = await _referenceRepository.ListAllAsync();
            return _mapper.Map<IReadOnlyList<ReferenceDto>>(references);
        }
    }
}
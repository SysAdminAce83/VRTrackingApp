using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.References.Queries
{
    public class GetReferenceByIdQueryHandler : IRequestHandler<GetReferenceByIdQuery, ReferenceDto>
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IMapper _mapper;

        public GetReferenceByIdQueryHandler(IReferenceRepository referenceRepository, IMapper mapper)
        {
            _referenceRepository = referenceRepository;
            _mapper = mapper;
        }

        public async Task<ReferenceDto> Handle(GetReferenceByIdQuery request, CancellationToken cancellationToken)
        {
            var reference = await _referenceRepository.GetByIdAsync(request.Id);
            return _mapper.Map<ReferenceDto>(reference);
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class CreateReferenceCommandHandler : IRequestHandler<CreateReferenceCommand, ReferenceDto>
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IMapper _mapper;

        public CreateReferenceCommandHandler(IReferenceRepository referenceRepository, IMapper mapper)
        {
            _referenceRepository = referenceRepository;
            _mapper = mapper;
        }

        public async Task<ReferenceDto> Handle(CreateReferenceCommand request, CancellationToken cancellationToken)
        {
            var reference = new Reference
            {
                VulnerabilityId = request.VulnerabilityId,
                ReferenceType = request.ReferenceType,
                ReferenceValue = request.ReferenceValue,
                URL = request.URL
                // Id and CreatedAt are set by the entity/database
            };

            await _referenceRepository.AddAsync(reference);
            return _mapper.Map<ReferenceDto>(reference);
        }
    }
}
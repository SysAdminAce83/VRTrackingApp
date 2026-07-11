using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class UpdateReferenceCommandHandler : IRequestHandler<UpdateReferenceCommand, Unit>
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IMapper _mapper;

        public UpdateReferenceCommandHandler(IReferenceRepository referenceRepository, IMapper mapper)
        {
            _referenceRepository = referenceRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateReferenceCommand request, CancellationToken cancellationToken)
        {
            var reference = await _referenceRepository.GetByIdAsync(request.Id);
            if (reference == null)
            {
                throw new KeyNotFoundException($"Reference with id {request.Id} not found");
            }

            reference.VulnerabilityId = request.VulnerabilityId;
            reference.ReferenceType = request.ReferenceType;
            reference.ReferenceValue = request.ReferenceValue;
            reference.URL = request.URL;

            await _referenceRepository.UpdateAsync(reference);
            return Unit.Value;
        }
    }
}
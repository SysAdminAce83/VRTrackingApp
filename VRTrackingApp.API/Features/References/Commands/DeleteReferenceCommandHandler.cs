using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.API.Features.References.Commands
{
    public class DeleteReferenceCommandHandler : IRequestHandler<DeleteReferenceCommand, Unit>
    {
        private readonly IReferenceRepository _referenceRepository;

        public DeleteReferenceCommandHandler(IReferenceRepository referenceRepository)
        {
            _referenceRepository = referenceRepository;
        }

        public async Task<Unit> Handle(DeleteReferenceCommand request, CancellationToken cancellationToken)
        {
            var reference = await _referenceRepository.GetByIdAsync(request.Id);
            if (reference == null)
            {
                throw new KeyNotFoundException($"Reference with id {request.Id} not found");
            }

            await _referenceRepository.DeleteAsync(reference);
            return Unit.Value;
        }
    }
}
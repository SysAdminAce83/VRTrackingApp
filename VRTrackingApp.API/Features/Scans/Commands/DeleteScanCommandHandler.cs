using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.API.Features.Scans.Commands
{
    public class DeleteScanCommandHandler : IRequestHandler<DeleteScanCommand>
    {
        private readonly IScanRepository _scanRepository;

        public DeleteScanCommandHandler(IScanRepository scanRepository)
        {
            _scanRepository = scanRepository;
        }

        public async Task Handle(DeleteScanCommand request, CancellationToken cancellationToken)
        {
            var scan = await _scanRepository.GetByIdAsync(request.Id);
            if (scan == null)
            {
                throw new KeyNotFoundException($"Scan with id {request.Id} not found");
            }

            await _scanRepository.DeleteAsync(scan);
        }
    }
}
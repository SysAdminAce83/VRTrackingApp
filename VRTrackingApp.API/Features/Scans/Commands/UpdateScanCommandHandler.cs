using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;

namespace VRTrackingApp.API.Features.Scans.Commands
{
    public class UpdateScanCommandHandler : IRequestHandler<UpdateScanCommand>
    {
        private readonly IScanRepository _scanRepository;

        public UpdateScanCommandHandler(IScanRepository scanRepository)
        {
            _scanRepository = scanRepository;
        }

        public async Task Handle(UpdateScanCommand request, CancellationToken cancellationToken)
        {
            var scan = await _scanRepository.GetByIdAsync(request.Id);
            if (scan == null)
            {
                throw new KeyNotFoundException($"Scan with id {request.Id} not found");
            }

            scan.ScanName = request.ScanName;
            scan.ScanDate = request.ScanDate;
            scan.ScanType = request.ScanType;
            scan.FileName = request.FileName;
            scan.FileSize = request.FileSize;
            scan.FileHash = request.FileHash;
            scan.Status = request.Status;
            scan.UpdatedAt = DateTime.UtcNow;

            await _scanRepository.UpdateAsync(scan);
        }
    }
}
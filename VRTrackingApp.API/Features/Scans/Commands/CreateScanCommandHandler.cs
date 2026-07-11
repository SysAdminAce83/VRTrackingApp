using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Scans.Commands
{
    public class CreateScanCommandHandler : IRequestHandler<CreateScanCommand, ScanDto>
    {
        private readonly IScanRepository _scanRepository;
        private readonly IMapper _mapper;

        public CreateScanCommandHandler(IScanRepository scanRepository, IMapper mapper)
        {
            _scanRepository = scanRepository;
            _mapper = mapper;
        }

        public async Task<ScanDto> Handle(CreateScanCommand request, CancellationToken cancellationToken)
        {
            var scan = new Scan
            {
                ScanName = request.ScanName,
                ScanDate = request.ScanDate,
                ScanType = request.ScanType,
                FileName = request.FileName,
                FileSize = request.FileSize,
                FileHash = request.FileHash,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _scanRepository.AddAsync(scan);
            return _mapper.Map<ScanDto>(scan);
        }
    }
}
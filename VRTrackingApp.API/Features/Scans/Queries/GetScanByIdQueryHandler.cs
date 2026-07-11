using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Scans.Queries
{
    public class GetScanByIdQueryHandler : IRequestHandler<GetScanByIdQuery, ScanDto>
    {
        private readonly IScanRepository _scanRepository;
        private readonly IMapper _mapper;

        public GetScanByIdQueryHandler(IScanRepository scanRepository, IMapper mapper)
        {
            _scanRepository = scanRepository;
            _mapper = mapper;
        }

        public async Task<ScanDto> Handle(GetScanByIdQuery request, CancellationToken cancellationToken)
        {
            var scan = await _scanRepository.GetByIdAsync(request.Id);
            return scan == null ? null! : _mapper.Map<ScanDto>(scan);
        }
    }
}
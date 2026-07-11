using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Domain.DTOs;
using AutoMapper;

namespace VRTrackingApp.API.Features.Scans.Queries
{
    public class GetScansQueryHandler : IRequestHandler<GetScansQuery, IReadOnlyList<ScanDto>>
    {
        private readonly IScanRepository _scanRepository;
        private readonly IMapper _mapper;

        public GetScansQueryHandler(IScanRepository scanRepository, IMapper mapper)
        {
            _scanRepository = scanRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ScanDto>> Handle(GetScansQuery request, CancellationToken cancellationToken)
        {
            var scans = await _scanRepository.ListAllAsync();
            return _mapper.Map<IReadOnlyList<ScanDto>>(scans);
        }
    }
}
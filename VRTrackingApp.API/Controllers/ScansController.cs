using MediatR;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.API.Features.Scans.Commands;
using VRTrackingApp.API.Features.Scans.Queries;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ScanDto>>> GetScans()
        {
            var query = new GetScansQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScanDto>> GetScan(int id)
        {
            var query = new GetScanByIdQuery(id);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ScanDto>> CreateScan(CreateScanCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetScan), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScan(int id, UpdateScanCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScan(int id)
        {
            var command = new DeleteScanCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
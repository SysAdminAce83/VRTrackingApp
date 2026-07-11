using MediatR;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.API.Features.References.Commands;
using VRTrackingApp.API.Features.References.Queries;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferencesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReferencesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ReferenceDto>>> GetReferences()
        {
            var query = new GetReferencesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReferenceDto>> GetReference(int id)
        {
            var query = new GetReferenceByIdQuery(id);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReferenceDto>> CreateReference(CreateReferenceCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetReference), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReference(int id, UpdateReferenceCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReference(int id)
        {
            var command = new DeleteReferenceCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("by-vulnerability/{vulnerabilityId}")]
        public async Task<ActionResult<IReadOnlyList<ReferenceDto>>> GetReferencesByVulnerabilityId(int vulnerabilityId)
        {
            var query = new GetReferencesByVulnerabilityIdQuery(vulnerabilityId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
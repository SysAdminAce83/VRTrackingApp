using MediatR;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.API.Features.Vulnerabilities.Commands;
using VRTrackingApp.API.Features.Vulnerabilities.Queries;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VulnerabilitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VulnerabilitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VulnerabilityDto>>> GetVulnerabilities()
        {
            var query = new GetVulnerabilitiesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VulnerabilityDto>> GetVulnerability(int id)
        {
            var query = new GetVulnerabilityByIdQuery(id);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<VulnerabilityDto>> CreateVulnerability(CreateVulnerabilityCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetVulnerability), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVulnerability(int id, UpdateVulnerabilityCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVulnerability(int id)
        {
            var command = new DeleteVulnerabilityCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("by-cve/{cve}")]
        public async Task<ActionResult<VulnerabilityDto>> GetVulnerabilityByCve(string cve)
        {
            var query = new GetVulnerabilityByCveQuery(cve);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("by-severity/{severity}")]
        public async Task<ActionResult<IReadOnlyList<VulnerabilityDto>>> GetVulnerabilitiesBySeverity(string severity)
        {
            var query = new GetVulnerabilitiesBySeverityQuery(severity);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
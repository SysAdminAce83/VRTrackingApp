using MediatR;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.API.Features.AssetVulnerabilities.Commands;
using VRTrackingApp.API.Features.AssetVulnerabilities.Queries;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetVulnerabilitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssetVulnerabilitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AssetVulnerabilityDto>>> GetAssetVulnerabilities()
        {
            var query = new GetAssetVulnerabilitiesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetVulnerabilityDto>> GetAssetVulnerability(int id)
        {
            var query = new GetAssetVulnerabilityByIdQuery(id);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AssetVulnerabilityDto>> CreateAssetVulnerability(CreateAssetVulnerabilityCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAssetVulnerability), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssetVulnerability(int id, UpdateAssetVulnerabilityCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetVulnerability(int id)
        {
            var command = new DeleteAssetVulnerabilityCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("by-asset/{assetId}")]
        public async Task<ActionResult<IReadOnlyList<AssetVulnerabilityDto>>> GetAssetVulnerabilitiesByAssetId(int assetId)
        {
            var query = new GetAssetVulnerabilitiesByAssetIdQuery(assetId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("by-vulnerability/{vulnerabilityId}")]
        public async Task<ActionResult<IReadOnlyList<AssetVulnerabilityDto>>> GetAssetVulnerabilitiesByVulnerabilityId(int vulnerabilityId)
        {
            var query = new GetAssetVulnerabilitiesByVulnerabilityIdQuery(vulnerabilityId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
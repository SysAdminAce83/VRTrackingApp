using MediatR;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.API.Features.Assets.Commands;
using VRTrackingApp.API.Features.Assets.Queries;
using VRTrackingApp.Domain.DTOs;

namespace VRTrackingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAssets()
        {
            var query = new GetAssetsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetDto>> GetAsset(int id)
        {
            var query = new GetAssetByIdQuery(id);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AssetDto>> CreateAsset(CreateAssetCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAsset), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, UpdateAssetCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var command = new DeleteAssetCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("by-scan/{scanId}")]
        public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAssetsByScanId(int scanId)
        {
            var query = new GetAssetsByScanIdQuery(scanId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("by-ip-address/{ipAddress}")]
        public async Task<ActionResult<AssetDto>> GetAssetByIpAddress(string ipAddress)
        {
            var query = new GetAssetByIpAddressQuery(ipAddress);
            var result = await _mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("by-host-name/{hostName}")]
        public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAssetsByHostName(string hostName)
        {
            var query = new GetAssetsByHostNameQuery(hostName);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
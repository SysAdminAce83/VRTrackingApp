using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.References.Queries
{
    public class GetReferencesQuery : IRequest<IReadOnlyList<ReferenceDto>>
    {
    }
}
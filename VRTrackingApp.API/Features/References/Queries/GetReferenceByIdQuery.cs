using MediatR;

using VRTrackingApp.Domain.DTOs;
namespace VRTrackingApp.API.Features.References.Queries
{
    public class GetReferenceByIdQuery : IRequest<ReferenceDto>
    {
        public int Id { get; }

        public GetReferenceByIdQuery(int id)
        {
            Id = id;
        }
    }
}
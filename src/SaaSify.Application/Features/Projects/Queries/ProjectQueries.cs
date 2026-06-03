using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Dtos;

namespace SaaSify.Application.Features.Projects.Queries;

public class GetProjectByIdQuery : Query<Result<ProjectResponse>>
{
    public Guid ProjectId { get; set; }
    public Guid OwnerId { get; set; } // Extraido del JWT
}

public class ListProjectsQuery : Query<Result<IReadOnlyList<ProjectResponse>>>
{
    public Guid OwnerId { get; set; } // Extraido del JWT
}
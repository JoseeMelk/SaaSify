using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Dtos;

namespace SaaSify.Application.Features.Projects.Commands;

public class CreateProjectCommand : Command<Result<CreateProjectResponse>>
{
    public Guid OwnerId { get; set; } // Extraido del JWT
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
}

public class RotateApiKeyCommand : Command<Result<RotateApiKeyResponse>>
{
    public Guid ProjectId { get; set; }
    public Guid OwnerId { get; set; } // Extraido del JWT
}
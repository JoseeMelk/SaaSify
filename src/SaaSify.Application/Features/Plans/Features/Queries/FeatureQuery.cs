using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Features.Dtos;

namespace SaaSify.Application.Features.Plans.Features.Queries;

/// <summary>
/// Query para listar las features de un plan por su slug.
/// 
/// Se usa el slug del plan en vez del ID porque es más amigable
/// para el developer — sabe el slug de su plan pero quizás no el ID.
/// </summary>
public class ListFeaturesQuery : Query<Result<IReadOnlyList<FeatureResponse>>>
{
    public Guid OwnerId { get; set; }    // del JWT
    public Guid ProjectId { get; set; } // de la URL
    public string PlanSlug { get; set; } = null!; // de la URL
}
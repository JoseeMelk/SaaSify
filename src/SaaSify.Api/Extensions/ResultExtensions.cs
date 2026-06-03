using Microsoft.AspNetCore.Mvc;
using SaaSify.Application.Common;

namespace SaaSify.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller)
    {
        if (!result.IsSuccess)
        {
            return controller.StatusCode(
                result.ErrorCode ?? 400,
                new
                {
                    error = result.Error,
                    errorCode = result.ErrorCode
                });
        }

        return controller.Ok(result.Data);
    }

    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller)
    {
        if (!result.IsSuccess)
        {
            return controller.StatusCode(
                result.ErrorCode ?? 400,
                new
                {
                    error = result.Error,
                    errorCode = result.ErrorCode
                });
        }

        return controller.Ok();
    }
}
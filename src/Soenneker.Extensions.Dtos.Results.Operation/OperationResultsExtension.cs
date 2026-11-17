using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soenneker.Dtos.ProblemDetails;
using Soenneker.Dtos.Results.Operation;

namespace Soenneker.Extensions.Dtos.Results.Operation;

/// <summary>
/// A collection of helpful OperationResult extension methods.
/// </summary>
public static class OperationResultsExtension
{
    private static readonly StatusCodeResult _noContentResult = new(StatusCodes.Status204NoContent);

    private static IActionResult ToResultCore<T>(OperationResult<T> response, bool useJsonResult)
    {
        if (!response.Succeeded)
            return ToFailureResult(response.StatusCode, response.Problem, useJsonResult);

        int statusCode = response.StatusCode;
        if (statusCode == StatusCodes.Status204NoContent)
            return _noContentResult;

        // Read and box the typed payload only when the response needs a body.
        return CreateResult(response.Value, statusCode == 0 ? StatusCodes.Status200OK : statusCode, useJsonResult);
    }

    private static IActionResult ToResultCore(OperationResult response, bool useJsonResult)
    {
        if (!response.Succeeded)
            return ToFailureResult(response.StatusCode, response.Problem, useJsonResult);

        int statusCode = response.StatusCode;
        if (statusCode == StatusCodes.Status204NoContent)
            return _noContentResult;

        return CreateResult(response.Value, statusCode == 0 ? StatusCodes.Status200OK : statusCode, useJsonResult);
    }

    private static IActionResult ToFailureResult(int statusCode, ProblemDetailsDto? problem, bool useJsonResult)
    {
        int fallbackStatusCode = statusCode == 0 ? StatusCodes.Status500InternalServerError : statusCode;
        ProblemDetailsDto pd = problem ?? new ProblemDetailsDto
        {
            Title = "Internal Server Error",
            Status = fallbackStatusCode
        };

        return CreateResult(pd, pd.Status ?? fallbackStatusCode, useJsonResult);
    }

    private static IActionResult CreateResult(object? value, int statusCode, bool useJsonResult) =>
        useJsonResult
            ? new JsonResult(value) { StatusCode = statusCode }
            : new ObjectResult(value) { StatusCode = statusCode };

    /// <summary>
    /// Converts the specified <see cref="OperationResult"/> into an <see cref="IActionResult"/> 
    /// suitable for use in ASP.NET Core MVC controllers.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value contained in the operation result.</typeparam>
    /// <param name="resp">The operation result to convert.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// For successful results (<see cref="OperationResult.Succeeded"/> is <c>true</c>):
    /// returns a 2xx result with the associated value, or a 204 No Content result if 
    /// <see cref="OperationResult.StatusCode"/> is 204.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// For failed results (<see cref="OperationResult.Succeeded"/> is <c>false</c>):
    /// returns a result containing the associated <see cref="OperationResult.Problem"/> details,
    /// or a default problem result if no details are provided.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    public static IActionResult ToActionResult<T>(this OperationResult<T> resp) =>
        ToResultCore(resp, false);

    /// <summary>
    /// Converts an operation result into an ASP.NET Core action result while preserving its success value or error response.
    /// </summary>
    /// <param name="resp">The operation result to translate.</param>
    /// <returns>The corresponding ASP.NET Core action result.</returns>
    public static IActionResult ToActionResult(this OperationResult resp) => ToResultCore(resp, false);

    /// <summary>
    /// Converts the operation result to a JSON-only action result, bypassing MVC content negotiation.
    /// </summary>
    /// <returns>Converts the operation result to a JSON-only action result, bypassing MVC content negotiation.</returns>
    public static IActionResult ToJsonResult<T>(this OperationResult<T> resp) =>
        ToResultCore(resp, true);

    /// <summary>
    /// Converts the operation result to a JSON-only action result, bypassing MVC content negotiation.
    /// </summary>
    /// <returns>Converts the operation result to a JSON-only action result, bypassing MVC content negotiation.</returns>
    public static IActionResult ToJsonResult(this OperationResult resp) => ToResultCore(resp, true);

    /// <summary>
    /// If the result failed, retypes it to TOut and preserves StatusCode/Problem.
    /// Throws if called on a successful result (use To/Map for that).
    /// </summary>
    /// <returns>If the result failed, retypes it to TOut and preserves StatusCode/Problem. Throws if called on a successful result (use To/Map for that).</returns>
    public static OperationResult<TOut> ToFailure<TOut>(this OperationResult resp)
    {
        if (resp.Succeeded)
            throw new InvalidOperationException("ToFailure<> should only be used on failed responses.");

        return new OperationResult<TOut>
        {
            StatusCode = resp.StatusCode,
            Problem = resp.Problem
        };
    }
}

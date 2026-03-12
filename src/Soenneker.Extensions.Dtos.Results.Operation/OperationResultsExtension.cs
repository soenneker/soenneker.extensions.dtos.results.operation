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
    private static IActionResult ToActionResultCore(bool succeeded, int statusCode, object? value, ProblemDetailsDto? problem)
    {
        if (succeeded)
        {
            if (statusCode == StatusCodes.Status204NoContent)
                return new StatusCodeResult(StatusCodes.Status204NoContent);

            // 2xx with body
            return new ObjectResult(value) { StatusCode = statusCode == 0 ? StatusCodes.Status200OK : statusCode };
        }

        // Ensure a ProblemDetails exists
        ProblemDetailsDto pd = problem ?? new ProblemDetailsDto
        {
            Title = "Internal Server Error",
            Status = statusCode == 0 ? StatusCodes.Status500InternalServerError : statusCode
        };

        return new ObjectResult(pd) { StatusCode = pd.Status ?? statusCode };
    }

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
    /// For successful results (<see cref="OperationResult{T}.Succeeded"/> is <c>true</c>):
    /// returns a 2xx result with the associated value, or a 204 No Content result if 
    /// <see cref="OperationResult{T}.StatusCode"/> is 204.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// For failed results (<see cref="OperationResult{T}.Succeeded"/> is <c>false</c>):
    /// returns a result containing the associated <see cref="OperationResult{T}.Problem"/> details, 
    /// or a default problem result if no details are provided.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    public static IActionResult ToActionResult<T>(this OperationResult<T> resp) =>
        ToActionResultCore(resp.Succeeded, resp.StatusCode, resp.Value, resp.Problem);

    ///<inheritdoc cref="ToActionResult{T}"/>
    public static IActionResult ToActionResult(this OperationResult resp) => ToActionResultCore(resp.Succeeded, resp.StatusCode, resp.Value, resp.Problem);

    /// <summary>
    /// If the result failed, retypes it to TOut and preserves StatusCode/Problem.
    /// Throws if called on a successful result (use To/Map for that).
    /// </summary>
    public static OperationResult<TOut> ToFailure<TOut>(this OperationResult resp)
    {
        if (resp.Succeeded)
            throw new InvalidOperationException("AsFailureOf<> should only be used on failed responses.");

        return new OperationResult<TOut>
        {
            StatusCode = resp.StatusCode,
            Problem = resp.Problem,
            Value = default
        };
    }
}

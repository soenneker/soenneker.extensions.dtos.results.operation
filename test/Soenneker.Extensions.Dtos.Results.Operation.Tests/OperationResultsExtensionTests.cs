using System;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soenneker.Dtos.ProblemDetails;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Tests.Unit;

namespace Soenneker.Extensions.Dtos.Results.Operation.Tests;

public sealed class OperationResultsExtensionTests : UnitTest
{
    [Test]
    public void ToActionResult_reuses_no_content_result()
    {
        var response = new OperationResult { StatusCode = StatusCodes.Status204NoContent };

        IActionResult first = response.ToActionResult();
        IActionResult second = response.ToActionResult();

        first.Should().BeSameAs(second);
    }

    [Test]
    public void ToActionResult_defaults_failure_status_code_to_internal_server_error()
    {
        var response = new OperationResult
        {
            Problem = new ProblemDetailsDto()
        };

        IActionResult result = response.ToActionResult();

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should()
              .Be(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public void ToJsonResult_returns_json_result_with_value_and_status_code()
    {
        var response = new OperationResult<string>
        {
            Value = "value",
            StatusCode = StatusCodes.Status201Created
        };

        IActionResult result = response.ToJsonResult();

        JsonResult jsonResult = result.Should().BeOfType<JsonResult>().Which;
        jsonResult.Value.Should().Be("value");
        jsonResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Test]
    public void ToJsonResult_reuses_no_content_result()
    {
        var response = new OperationResult { StatusCode = StatusCodes.Status204NoContent };

        IActionResult jsonResult = response.ToJsonResult();
        IActionResult actionResult = response.ToActionResult();

        jsonResult.Should().BeSameAs(actionResult);
    }
    [Test]
    public void Generic_no_content_conversions_do_not_allocate()
    {
        var response = new OperationResult<Guid>
        {
            Value = Guid.NewGuid(),
            StatusCode = StatusCodes.Status204NoContent
        };

        // Warm up both paths, including the shared result's static initialization.
        IActionResult expected = response.ToActionResult();
        response.ToJsonResult();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
        {
            response.ToActionResult();
            response.ToJsonResult();
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.Should().Be(0);
        response.ToActionResult().Should().BeSameAs(expected);
        response.ToJsonResult().Should().BeSameAs(expected);
    }

    [Test]
    public void Generic_success_uses_typed_payload_and_defaults_to_ok()
    {
        var response = new OperationResult<int> { Value = 42 };
        ((OperationResult)response).Value = "base payload";

        ObjectResult actionResult = response.ToActionResult().Should().BeOfType<ObjectResult>().Which;
        JsonResult jsonResult = response.ToJsonResult().Should().BeOfType<JsonResult>().Which;

        actionResult.Value.Should().Be(42);
        jsonResult.Value.Should().Be(42);
        actionResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        jsonResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public void Generic_failure_preserves_problem_and_its_status_even_with_no_content_status()
    {
        var problem = new ProblemDetailsDto { Status = StatusCodes.Status409Conflict };
        var response = new OperationResult<Guid>
        {
            Value = Guid.NewGuid(),
            StatusCode = StatusCodes.Status204NoContent,
            Problem = problem
        };

        ObjectResult actionResult = response.ToActionResult().Should().BeOfType<ObjectResult>().Which;
        JsonResult jsonResult = response.ToJsonResult().Should().BeOfType<JsonResult>().Which;

        actionResult.Value.Should().BeSameAs(problem);
        jsonResult.Value.Should().BeSameAs(problem);
        actionResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        jsonResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }
}

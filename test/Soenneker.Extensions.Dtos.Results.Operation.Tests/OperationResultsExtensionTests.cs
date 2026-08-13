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
}

[![](https://img.shields.io/nuget/v/soenneker.extensions.dtos.results.operation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.dtos.results.operation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dtos.results.operation/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dtos.results.operation/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.dtos.results.operation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.dtos.results.operation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dtos.results.operation/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dtos.results.operation/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Dtos.Results.Operation

Converts `OperationResult` values into ASP.NET Core action results and retypes failed results for early-return pipelines.

## Installation

```bash
dotnet add package Soenneker.Extensions.Dtos.Results.Operation
```

## Controller results

```csharp
using Microsoft.AspNetCore.Mvc;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Extensions.Dtos.Results.Operation;

public IActionResult GetCustomer()
{
    OperationResult<Customer> result = LoadCustomer();
    return result.ToActionResult();
}
```

`ToActionResult()` returns `ObjectResult`, allowing normal MVC content negotiation. `ToJsonResult()` returns `JsonResult` to force JSON. Both generic and non-generic `OperationResult` values are supported.

An operation is considered successful when `Problem` is null; the numeric status code alone does not determine success.

### Success responses

- Status code `0` becomes `200 OK`.
- Status code `204` becomes a bodyless `StatusCodeResult`; any value is ignored.
- Every other status code is used as supplied, with `Value` as the response body.

### Failure responses

- The existing `ProblemDetailsDto` is returned as the body.
- `Problem.Status` is the HTTP status when it is present.
- Otherwise, the operation result’s nonzero `StatusCode` is used.
- If both status values are absent or zero, the HTTP status is `500`.

The conversion does not clone or normalize an existing problem object. In particular, a null `Problem.Status` remains null in the body even when the HTTP response defaults to 500.

## Retype a failure

Use `ToFailure<TOut>()` when a method returning `OperationResult<TOut>` needs to pass through an earlier non-generic failure:

```csharp
OperationResult validation = ValidateRequest();

if (validation.Failed)
    return validation.ToFailure<Customer>();
```

The new result preserves `StatusCode` and the same `Problem` instance. Calling `ToFailure<TOut>()` on a successful result throws `InvalidOperationException`; use a normal mapping operation for successful values.

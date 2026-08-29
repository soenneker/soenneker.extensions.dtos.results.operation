[![](https://img.shields.io/nuget/v/soenneker.extensions.dtos.results.operation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.dtos.results.operation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dtos.results.operation/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dtos.results.operation/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.dtos.results.operation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.dtos.results.operation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dtos.results.operation/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dtos.results.operation/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Dtos.Results.Operation
A collection of helpful OperationResult extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.Dtos.Results.Operation
```

## Quick start

```csharp
using Soenneker.Extensions.Dtos.Results.Operation;

// Given an existing OperationResult<T> named resp:
var result = resp.ToActionResult();
```

## Common operations

- `ToActionResult()` - Converts the specified `OperationResult` into an `IActionResult` suitable for use in ASP.NET Core MVC controllers.
- `ToJsonResult()` - Converts the operation result to a JSON-only action result, bypassing MVC content negotiation.
- `ToFailure()` - If the result failed, retypes it to TOut and preserves StatusCode/Problem. Throws if called on a successful result (use To/Map for that).

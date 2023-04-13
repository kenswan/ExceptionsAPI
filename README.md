[![Nuget Version](https://img.shields.io/nuget/v/ExceptionsAPI?logo=nuget)](https://www.nuget.org/packages/ExceptionsAPI)
[![Nuget Downloads](https://img.shields.io/nuget/dt/ExceptionsAPI?logo=nuget)](https://www.nuget.org/packages/ExceptionsAPI)
[![Continuous Integration](https://github.com/kenswan/ExceptionsAPI/actions/workflows/continuous-integration.yml/badge.svg)](https://github.com/kenswan/ExceptionsAPI/actions/workflows/continuous-integration.yml)

# ExceptionsAPI

**Standardizing Exception Handling Middleware to enhance API Developer experience.**

ExceptionsAPI allows applications to determine status codes and error messages for exceptions thrown in the application. This allows the application to be more flexible in how it handles exceptions and allows the application to be more consistent in how it handles exceptions.

## Quick Start

### Configuration

Register Exception Status Codes (Program.cs or Startup.cs)

```csharp
builder.Services
    .AddExceptionsAPI()
        .AddException<YourCustomOrBuiltInException>(HttpStatusCode.GatewayTimeout);
```

Activate Exception Handling Middleware (Program.cs or Startup.cs)

```csharp
app.UseExceptionsAPI();
```

### Behavior

With the setup in the previous section, now throwing your `YourCustomOrBuiltInException` exception type will generate the following output in the response body:

Exception Code:

```csharp
throw new YourCustomOrBuiltInException("This Message Will Get Logged And Returned");
```

Http Response Body (ProblemDetails):

```json
{
  "type": "YourCustomOrBuiltInException",
  "title": "GatewayTimeout",
  "status": 504,
  "detail": "This Message Will Get Logged And Returned",
  "instance": "/api/YourEndpoint/PathAndQueryRequest?IncludingParams=true",
  "correlationId": "782e230a-7fb7-43aa-8d64-56d27c9f0e27"
}
```

ADDITIONALLY, adding `Exception.Data` to show multiple errors will be returned as below:

Exception Code:

```csharp
var exceptionToBeThrown = new YourCustomOrBuiltInException("This Message Will Get Logged And Returned");

exceptionToBeThrown.Data.Add("TestField1", "This Failed for some reason");
exceptionToBeThrown.Data.Add("TestField2", "This Failed for some other reason");
exceptionToBeThrown.Data.Add("TestField3", "This Failed for another reason");

throw exceptionToBeThrown;
```

Http Response Body (ValidationProblemDetails):

```json
{
  "type": "YourCustomOrBuiltInException",
  "title": "GatewayTimeout",
  "status": 504,
  "detail": "This Message Will Get Logged And Returned",
  "instance": "/api/YourEndpoint/PathAndQueryRequest?IncludingParams=true",
  "correlationId": "da3f8dde-e4a0-4932-9508-36e52cbad480",
  "errors": {
    "TestField1": ["This Failed for some reason"],
    "TestField2": ["This Failed for some other reason"],
    "TestField3": ["This Failed for another reason"]
  }
}
```

Full Setup Sample (Program.cs)

```csharp
using ExceptionsAPI;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom exceptions and status codes
builder.Services
    .AddExceptionsAPI()
        // Chain multiple exceptions to the same status code to various status codes
        .AddException<YourCustomOrBuiltInException>(HttpStatusCode.GatewayTimeout);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Register Exceptions API Middleware
app.UseExceptionsAPI();
```

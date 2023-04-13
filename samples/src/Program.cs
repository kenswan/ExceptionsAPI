// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI;
using Samples.ExceptionsAPI.Exceptions;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom exceptions and status codes
builder.Services
    .AddExceptionsAPI()
        .AddException<RandomException>(HttpStatusCode.Ambiguous);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Register Exceptions API Middleware
app.UseExceptionsAPI();

// Endpoints
app.MapGet("/ThrowRandomException", () =>
{
    throw new RandomException("This is a test exception");
})
.WithOpenApi();

app.Run();

// Used for integration test web application factory accessibility
public partial class Program { }

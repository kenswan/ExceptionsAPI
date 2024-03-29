﻿// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI;
using Microsoft.AspNetCore.Mvc;
using Samples.ExceptionsAPI.Exceptions;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom exceptions and status codes
builder.Services
    // .AddExceptionsAPI() -> Use this extension for  default correlation key/value
    .AddExceptionsAPI(options =>
    {
        options.CorrelationKey = "X-TestCorrelation-Id";
        options.CorrelationKey = CORRELATION_HEADER_KEY;
        options.ConfigureCorrelationValue = (httpContext) => { return httpContext.TraceIdentifier; };
    }) // Use this extension for  default correlation key/value
        .AddException<RandomException>(HttpStatusCode.FailedDependency);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Register Exceptions API Middleware
app.UseExceptionsAPI();

// Endpoints
app.MapGet("/ThrowRandomException", () =>
{
    throw new RandomException();
})
.WithOpenApi();

app.MapGet("/ThrowCustomClientException", (
    [FromQuery] HttpStatusCode? statusCode,
    [FromQuery] string? message,
    [FromQuery] string? clientMessage) =>
{
    var emptyMessage = "No Message Sent";

    CustomClientException exception = (statusCode, message) switch
    {
        { statusCode: null, message: null } => new CustomClientException(emptyMessage) { ClientErrorMessage = clientMessage },

        { statusCode: null } => new CustomClientException(message) { ClientErrorMessage = clientMessage },

        { message: null } => new CustomClientException(statusCode.Value, emptyMessage) { ClientErrorMessage = clientMessage },

        _ => new CustomClientException(statusCode.Value, message) { ClientErrorMessage = clientMessage }
    };

    throw exception;
})
.WithOpenApi();

app.Run();

// Used for integration test web application factory accessibility
public partial class Program
{
    public const string CORRELATION_HEADER_KEY = "Test-Correlation-Id";
}

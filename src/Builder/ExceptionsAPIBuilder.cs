// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace ExceptionsAPI.Builder;

internal class ExceptionsAPIBuilder : IExceptionsAPIBuilder
{
    private readonly IServiceCollection serviceCollection;

    public ExceptionsAPIBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public IExceptionsAPIBuilder AddCorrelation(Func<HttpContext, IServiceProvider, string> correlationBuilder) =>
        throw new NotImplementedException();

    public IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                options.ExceptionType = typeof(TException);
                options.HttpStatusCode = httpStatusCode;
                options.ExceptionResponseResolver = null;
            });

        return this;
    }

    public IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode, string clientErrorMessage)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                options.ExceptionType = typeof(TException);
                options.HttpStatusCode = httpStatusCode;
                options.DefaultMessage = clientErrorMessage;
            });

        return this;
    }

    public IExceptionsAPIBuilder AddException<TException>(Func<HttpContext, TException, ExceptionsAPIResponse> buildExceptionResponse)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                var resolver = new ExceptionResponseResolver<TException>(buildExceptionResponse);

                options.ExceptionType = typeof(TException);
                options.ExceptionResponseResolver = resolver;
                options.DefaultMessage = null;
            });

        return this;
    }
}

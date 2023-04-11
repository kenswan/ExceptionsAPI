// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace ExceptionsAPI;

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
                options.ExceptionMapping = null;
            });

        return this;
    }

    public IExceptionsAPIBuilder AddException<TException>(Func<TException, ProblemDetails> action)
        where TException : Exception =>
            throw new NotImplementedException();
}

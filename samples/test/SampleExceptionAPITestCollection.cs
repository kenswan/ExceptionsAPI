// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;

namespace Samples.ExceptionsAPI.Test;

[CollectionDefinition(nameof(SampleExceptionAPITestCollection))]
public class SampleExceptionAPITestCollection : ICollectionFixture<WebApplicationFactory<Program>>
{ }

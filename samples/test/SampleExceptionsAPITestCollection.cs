// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;

namespace Samples.ExceptionsAPI.Test;

[CollectionDefinition(nameof(SampleExceptionsAPITestCollection))]
public class SampleExceptionsAPITestCollection : ICollectionFixture<WebApplicationFactory<Program>>
{ }

// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

namespace Samples.ExceptionAPI.Exceptions;

public class RandomException : Exception
{
    public RandomException(string message) : base(message) { }

    public RandomException(string message, Exception innerException) : base(message, innerException) { }
}

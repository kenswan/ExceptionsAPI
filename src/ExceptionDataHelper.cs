// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace ExceptionsAPI;

internal static class ExceptionDataHelper
{
    public static void SetValidationDetailsFromExceptionData(ValidationProblemDetails problemDetails, Exception exception)
    {
        foreach (var key in exception.Data.Keys)
        {
            var data = exception.Data[key];

            var values = data switch
            {
                { } when data is string singleEntry => new string[] { singleEntry },

                { } when data is IList<string> listOfValues => listOfValues.ToArray(),

                { } when data is IList generalList => GetArrayOfStringsFromObjects(generalList),

                { } when data is not null => GetArrayOfStringsFromSingleObject(data),

                _ => Array.Empty<string>()
            };

            problemDetails.Errors.TryAdd(key.ToString(), values);
        }
    }
    private static string[] GetArrayOfStringsFromObjects(IList list)
    {
        var values = new List<string>();

        foreach (var item in list)
        {
            values.Add(JsonSerializer.Serialize(item));
        }

        return values.ToArray();
    }

    private static string[] GetArrayOfStringsFromSingleObject(object inputObject)
    {
        var value = JsonSerializer.Serialize(inputObject);

        return new string[] { value };
    }
}

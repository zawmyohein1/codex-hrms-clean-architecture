using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HRMS.UI.Infrastructure;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task AddErrorsToModelStateAsync(this HttpResponseMessage response, ModelStateDictionary modelState, string entityName)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (modelState is null)
        {
            throw new ArgumentNullException(nameof(modelState));
        }

        var fallbackMessage = $"Unable to save {entityName}.";

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                modelState.AddModelError(string.Empty, fallbackMessage);
                return;
            }

            var validation = JsonSerializer.Deserialize<ValidationProblemDetails>(content, JsonOptions);
            if (validation?.Errors?.Count > 0)
            {
                foreach (var (key, errors) in validation.Errors)
                {
                    var targetKey = string.IsNullOrWhiteSpace(key) ? string.Empty : key;
                    foreach (var error in errors)
                    {
                        modelState.AddModelError(targetKey, error);
                    }
                }

                var summaryMessage = !string.IsNullOrWhiteSpace(validation.Detail)
                    ? validation.Detail
                    : validation.Title;

                if (!string.IsNullOrWhiteSpace(summaryMessage))
                {
                    modelState.TryAddModelError(string.Empty, summaryMessage);
                }

                return;
            }

            var problem = JsonSerializer.Deserialize<ProblemDetails>(content, JsonOptions);
            if (problem is not null)
            {
                var message = !string.IsNullOrWhiteSpace(problem.Detail)
                    ? problem.Detail
                    : problem.Title;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    modelState.AddModelError(string.Empty, message);
                }
                else
                {
                    modelState.AddModelError(string.Empty, fallbackMessage);
                }

                return;
            }

            modelState.AddModelError(string.Empty, fallbackMessage);
        }
        catch (JsonException)
        {
            modelState.AddModelError(string.Empty, fallbackMessage);
        }
    }
}

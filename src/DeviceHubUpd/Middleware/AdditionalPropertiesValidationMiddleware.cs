namespace DeviceHubUpd.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

public class AdditionalPropertiesValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdditionalPropertiesValidationMiddleware> _logger;
    private readonly List<ValidationRule> _rules;

    public AdditionalPropertiesValidationMiddleware(RequestDelegate next, ILogger<AdditionalPropertiesValidationMiddleware> logger, IConfiguration config)
    {
        _next = next;
        _logger = logger;
        var json = File.ReadAllText("/Users/danylooliinyk/uni/apbd/DeviceHubUpd/src/DeviceHubUpd/Resources/validationRules.json");
        _rules = JsonSerializer.Deserialize<ValidationRulesFile>(json).Validations;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Starting additional properties validation middleware.");

        // only validate on device create/update
        if ((context.Request.Path.StartsWithSegments("/api/devices") && 
            (context.Request.Method == "POST" || context.Request.Method == "PUT")))
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            // get type and isEnabled
            var typeId = root.GetProperty("typeId").GetInt32();
            var isEnabled = root.GetProperty("isEnabled").GetBoolean();
            var additionalProps = root.GetProperty("additionalProperties").GetRawText();

            // map typeId to typeName (you may need to query DB or cache)
            // for demo, assume typeName = "PC"
            var typeName = "PC";

            var rule = _rules.FirstOrDefault(r => r.Type == typeName && r.PreRequestName == "isEnabled" && r.PreRequestValue == isEnabled.ToString().ToLower());
            if (rule != null)
            {
                var addPropsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(additionalProps);
                foreach (var r in rule.Rules)
                {
                    if (addPropsDict.TryGetValue(r.ParamName, out var value))
                    {
                        if (r.Regex.StartsWith("/") && r.Regex.EndsWith("/"))
                        {
                            var regex = new Regex(r.Regex.Trim('/'));
                            if (!regex.IsMatch(value.ToString()))
                            {
                                _logger.LogWarning("Validation failed for {ParamName}", r.ParamName);
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsync($"Validation failed for {r.ParamName}");
                                return;
                            }
                        }
                        else if (r.Regex.StartsWith("[") && r.Regex.EndsWith("]"))
                        {
                            var allowed = JsonSerializer.Deserialize<List<string>>(r.Regex);
                            if (!allowed.Contains(value.ToString()))
                            {
                                _logger.LogWarning("Validation failed for {ParamName}", r.ParamName);
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsync($"Validation failed for {r.ParamName}");
                                return;
                            }
                        }
                        else
                        {
                            var regex = new Regex(r.Regex);
                            if (!regex.IsMatch(value.ToString()))
                            {
                                _logger.LogWarning("Validation failed for {ParamName}", r.ParamName);
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsync($"Validation failed for {r.ParamName}");
                                return;
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
        _logger.LogInformation("Finished additional properties validation middleware.");
    }
}

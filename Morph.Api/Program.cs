using Microsoft.AspNetCore.Mvc;
using Morph;
using Scalar.AspNetCore;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapPost("/{url}", async (string url, HttpContext httpContext, HttpRequest httpRequest, [FromBody] JsonObject json) =>
{
	var morphProgram = GetMorphForUrl(url);

	if (morphProgram is null)
	{
		httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
		await httpContext.Response.WriteAsync("Unable to find program at url: " + url);
		return;
	}

	var inputs = new Dictionary<string, string>()
	{
		{ "body", json.ToJsonString() },
		{ "url", httpContext.Request.Host + httpContext.Request.Path + httpContext.Request.QueryString },
		{ "headers", string.Join(";", httpContext.Request.Headers.Select(h => $"{h.Key}:{h.Value}")) }
	};

	var stdOut = new StringBuilder();
	var errorOut = new StringBuilder();
	var headers = new Dictionary<string, string>();
	int? responseStatusCode = null;

	void Handler(object? _, Message message)
	{
		switch (message.channel)
		{
			case "write":
				stdOut.Append(message.content);
				break;
			case "error":
				errorOut.Append(message.content);
				break;
			case "debug":
				Console.WriteLine(message.content);
				break;
			case "header":
				var parts = message.content.Split(':', 2);
				if (parts.Length == 2)
				{
					headers[parts[0].Trim()] = parts[1].Trim();
				}
				break;
			case "response":
				if (int.TryParse(message.content, out int statusCode))
				{
					responseStatusCode = statusCode;
				}
				else
				{
					responseStatusCode = (int)HttpStatusCode.InternalServerError;
					errorOut.Append("Invalid response code set: " + message.content);
				}
				break;
		}
	}

	// Subscribe to the event
	Morph.Morph.Out += Handler;

	try
	{
		var result = Morph.Morph.RunCode(morphProgram, inputs);

		// Apply headers safely before writing the response
		foreach (var kvp in headers)
		{
			httpContext.Response.Headers[kvp.Key] = kvp.Value;
		}

		httpContext.Response.StatusCode = responseStatusCode ?? (result ? 200 : 500);

		if (result)
		{
			await httpContext.Response.WriteAsync(stdOut.ToString());
		}
		else
		{
			Console.Error.WriteLine(errorOut.ToString());
			await httpContext.Response.WriteAsync(errorOut.ToString());
		}
	}
	finally
	{
		// Always unsubscribe after
		Morph.Morph.Out -= Handler;
	}
});

app.Run();

string? GetMorphForUrl(string url)
{
	var filePath = Directory.GetCurrentDirectory() + "\\morph\\" + url.Trim('/') + ".mor";

	if (File.Exists(filePath))
	{
		return File.ReadAllText(filePath);
	}

	return null;
}
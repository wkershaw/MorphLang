using Microsoft.AspNetCore.Mvc;
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
		return;// Results.NotFound("Program not found");
	}

	var inputs = new Dictionary<string, string>()
	{
		{ "body", json.ToJsonString() },
		{ "query", httpRequest.QueryString.ToString() }
	};

	var stdOut = new StringBuilder();
	var errorOut = new StringBuilder();

	Morph.Morph.StdOut += (object? _, string message) => stdOut.Append(message);
	Morph.Morph.ErrorOut += (object? _, string message) => errorOut.Append(message);
	Morph.Morph.DebugOut += (object? _, string message) => Console.WriteLine(message);

	var result = Morph.Morph.RunCode(morphProgram, inputs);

	if (result)
	{
		await httpContext.Response.WriteAsync(stdOut.ToString());
		return;
	}

	Console.Error.WriteLine(errorOut.ToString());

	httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
	await httpContext.Response.WriteAsync(errorOut.ToString());
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
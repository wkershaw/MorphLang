using Microsoft.AspNetCore.Mvc;
using Morph;
using System.Text;
using System.Text.Json.Nodes;

var app = WebApplication
	.CreateBuilder(args)
	.Build();

app.MapPost("/{url}", HandleRequest);
app.MapGet("/{url}", HandleRequest);
app.MapPatch("/{url}", HandleRequest);
app.MapPut("/{url}", HandleRequest);

app.Run();

async Task HandleRequest(string url, HttpContext httpContext, [FromBody] JsonObject json)
{
	string? program = MorphFileResolver.GetMorphForUrl(url);

	if (program is null)
	{
		httpContext.Response.StatusCode = 400;
		return;	
	}

	var headers = httpContext.Request.Headers;
	var inputs = new Dictionary<string, string>()
	{
		{ "body", json.ToJsonString() },
		{ "url", url },
		{ "headers", string.Join(";", headers.Select(h => $"{h.Key}:{h.Value}")) }
	};

	var outputSb = new StringBuilder();

	MorphRunner.Out += (sender, message) =>
	{
		switch (message.channel)
		{
			case "error":
				Console.Error.WriteLine(message.content);
				break;

			case "write":
				outputSb.Append(message.content);
				break;

			case "response":
				httpContext.Response.StatusCode = int.Parse(message.content);
				break;

			case "header":
				Console.WriteLine("TODO: need to add header functionality!!: " + message.content);
				break;

			case "debug":
			default:
				Console.WriteLine(message.content);
				break;
		}
	};

	var runResult = MorphRunner.RunCode(program, inputs);

	if (!runResult)
	{
		httpContext.Response.StatusCode = 500;
	}

	await httpContext.Response.WriteAsync(outputSb.ToString());
}
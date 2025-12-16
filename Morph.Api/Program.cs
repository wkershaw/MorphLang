
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

var app = WebApplication
	.CreateBuilder(args)
	.Build();


app.MapPost("/{url}", async (string url, HttpContext httpContext, [FromBody] JsonObject json) =>
{
	var runner = new MorphRunner();
	var runResult = await runner.RunMorph(url, json.ToJsonString(), httpContext.Request.Headers); 

	switch(runResult)
	{
		case NotFoundResult:
			httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await httpContext.Response.WriteAsync("Unable to find program at url: " + url);
			break;

		case ErrorResult errorResult:
			httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			await httpContext.Response.WriteAsync(errorResult.Ex.Message);
			break;

		case SuccessResult successResult:
			httpContext.Response.StatusCode = successResult.ResponseStatusCode ?? (int)HttpStatusCode.OK;
			await httpContext.Response.WriteAsync(successResult.StdOut);
			break;

		default:
			throw new Exception("Unexpected run result");
	}

});

app.Run();
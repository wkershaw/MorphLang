using System.Diagnostics;
using System.Text;
using Morph;

public interface IMorphRunResult {};

public class NotFoundResult : IMorphRunResult {}

public class SuccessResult : IMorphRunResult
{
    public required string StdOut { get; init;}
    public required string Errors { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public required int? ResponseStatusCode { get; init; } 
}

public class ErrorResult(Exception ex) : IMorphRunResult
{
    public Exception Ex { get; init; } = ex;
}

public class MorphRunner
{
    private readonly MorphFileResolver _morphResolver = new MorphFileResolver();
    private readonly StringBuilder _stdOut = new StringBuilder();
    private readonly StringBuilder _errorOut = new StringBuilder();
    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
    private int? _responseStatusCode = null;

    public async Task<IMorphRunResult> RunMorph(string url, string json, IHeaderDictionary headers)
    {
        var morphProgram = _morphResolver.GetMorphForUrl(url);

        if (morphProgram is null)
        {
            return new NotFoundResult();
        }

        var inputs = new Dictionary<string, string>()
        {
            { "body", json },
            { "url", url },
            { "headers", string.Join(";", headers.Select(h => $"{h.Key}:{h.Value}")) }
        };

        // Subscribe to the message events
        Morph.Morph.Out += MessageHandler;

        try
        {
            var result = Morph.Morph.RunCode(morphProgram, inputs);
        }
        catch(Exception ex)
        {
            return new ErrorResult(ex);
        }
        finally
        {
            Morph.Morph.Out -= MessageHandler;
        }

        var successResult = new SuccessResult()
        {
            StdOut = _stdOut.ToString(),
            Errors = _errorOut.ToString(),
            Headers = _headers,
            ResponseStatusCode = _responseStatusCode
        };

        return successResult;
    }

    private void MessageHandler(object? _, Message message)
    {
        switch (message.channel)
        {
            case "write":
                _stdOut.Append(message.content);
                break;

            case "error":
                _errorOut.Append(message.content);
                break;

            case "debug":
                Console.WriteLine(message.content);
                break;

            case "header":
                var parts = message.content.Split(':', 2);
                Debug.Assert(parts.Length == 2, "Header is not composed of 2 parts: " + message.content);
                _headers[parts[0].Trim()] = parts[1].Trim();
                break;

            case "response":
                int statusCode = int.Parse(message.content);
                _responseStatusCode = statusCode;
                
                break;
        }
    }
}
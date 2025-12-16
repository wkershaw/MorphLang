public class MorphFileResolver
{
    public string? GetMorphForUrl(string url)
    {
        var filePath = Directory.GetCurrentDirectory() + "\\morph\\" + url.Trim('/') + ".mor";

        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }

        return null;
    }
}
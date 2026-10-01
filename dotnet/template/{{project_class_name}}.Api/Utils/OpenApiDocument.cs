namespace {{project_class_name}}.Api.Utils;

using System;
using System.IO;

public static class OpenApiDocument
{
    public static readonly string Json = Load();

    private static string Load()
    {
        using var stream = typeof(OpenApiDocument).Assembly.GetManifestResourceStream("openapi.json")
            ?? throw new InvalidOperationException("openapi.json is not embedded in the assembly.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

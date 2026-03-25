namespace TaskManager.Api.Options;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string[] Origins { get; init; } = [];

    public string[] GetNormalizedOrigins()
    {
        return Origins
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

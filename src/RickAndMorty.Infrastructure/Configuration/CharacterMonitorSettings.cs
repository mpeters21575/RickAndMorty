namespace RickAndMorty.Infrastructure.Configuration;

public sealed class CharacterMonitorSettings
{
    public const string SectionName = "CharacterMonitor";

    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 5;
    public bool TestMode { get; set; } = false;
}
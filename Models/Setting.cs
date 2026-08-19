// csharp
namespace QuranKemenag.Models;

/// <summary>
/// Represents a simple key/value setting entry.
/// </summary>
public class Setting
{
    /// <summary>
    /// Key of the setting.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Value of the setting.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
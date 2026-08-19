using System;

namespace QuranKemenag.Models;

/// <summary>
/// Represents a display mode option shown in the toolbar ComboBox.
/// This is a simple DTO used by the ViewModel to present user-friendly
/// labels and optional icons while keeping the underlying enum value.
/// </summary>
public sealed class DisplayModeItem
{
    public DisplayModeItem() { }

    public DisplayModeItem(DisplayMode mode, string displayName, string? icon = null)
    {
        Mode = mode;
        DisplayName = displayName;
        Icon = icon;
    }

    /// <summary>
    /// Mode tampilan Al-Qur'an.
    /// </summary>
    public DisplayMode Mode { get; set; }

    /// <summary>
    /// Friendly label shown in the ComboBox.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional icon (emoji or glyph) displayed to the left of the label.
    /// </summary>
    public string? Icon { get; set; }

    public override string ToString() => string.IsNullOrEmpty(Icon) ? DisplayName : Icon + " " + DisplayName;
}

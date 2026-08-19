namespace QuranKemenag.Models;

public sealed class DisplayPreset
{
    public string Name { get; }

    public double ArabicFontSize { get; }

    public double TranslationFontSize { get; }

    public double IsyaratFontSize { get; }

    public bool ShowTranslation { get; }

    public bool ShowIsyarat { get; }


    public DisplayPreset(
        string name,
        double arabicFontSize,
        double translationFontSize,
        double isyaratFontSize,
        bool showTranslation,
        bool showIsyarat)
    {
        Name = name;

        ArabicFontSize =
            arabicFontSize;

        TranslationFontSize =
            translationFontSize;

        IsyaratFontSize =
            isyaratFontSize;

        ShowTranslation =
            showTranslation;

        ShowIsyarat =
            showIsyarat;
    }


    public override string ToString()
    {
        return Name;
    }
}
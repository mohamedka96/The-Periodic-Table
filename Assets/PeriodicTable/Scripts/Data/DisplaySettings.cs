using UnityEngine;

[CreateAssetMenu(fileName = "DisplaySettings", menuName = "Periodic Table/Display Settings")]
public class DisplaySettings : ScriptableObject
{
    [Header("Display sections")]
    public bool showTitle = true;
    public bool showFacts = true;
    public bool showDescription = true;

    [Header("Text templates")]
    [ArabicTextArea(2, 4)]
    public string titleTemplate = "{number} - ({symbol})\n{name}";

    [ArabicTextArea(4, 10)]
    public string factsTemplate =
        "البروتونات: {protons}\n" +
        "النيوترونات: {neutrons}\n" +
        "الإلكترونات: {electrons}\n" +
        "الكتلة الذرية: {mass}\n" +
        "التصنيف: {category}\n" +
        "توزيع الإلكترونات: {shells}";

    [ArabicTextArea(2, 6)]
    public string descriptionTemplate = "{description}";

    [Header("Text style")]
    public float titleFontSize = 0.1f;
    public Color titleColor = Color.white;
    public float bodyFontSize = 0.1f;
    public Color bodyColor = Color.white;

    public void ApplyTo(ElementEntry element, out string title, out string body)
    {
        title = showTitle ? FormatTemplate(titleTemplate, element) : string.Empty;
        body = BuildBody(element);
    }

    string BuildBody(ElementEntry element)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (showFacts) parts.Add(FormatTemplate(factsTemplate, element));
        if (showDescription) parts.Add(FormatTemplate(descriptionTemplate, element));
        return string.Join("\n\n", parts);
    }

    public static string FormatTemplate(string template, ElementEntry element)
    {
        if (string.IsNullOrEmpty(template) || element == null) return string.Empty;

        return template
            .Replace("{number}", element.atomicNumber.ToString())
            .Replace("{symbol}", element.symbol)
            .Replace("{name}", element.nameArabic)
            .Replace("{protons}", element.Protons.ToString())
            .Replace("{neutrons}", element.Neutrons.ToString())
            .Replace("{electrons}", element.Electrons.ToString())
            .Replace("{mass}", element.atomicMass.ToString("0.###"))
            .Replace("{category}", element.categoryArabic)
            .Replace("{description}", element.descriptionArabic)
            .Replace("{shells}", AtomicDataUtility.GetShellDistributionText(element.Electrons));
    }
}

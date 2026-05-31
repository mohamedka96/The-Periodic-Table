using UnityEngine;

/// <summary>
/// حقل نص عربي — يُعرض في Inspector مع معاينة مثل اللعبة.
/// </summary>
public class ArabicTextAreaAttribute : PropertyAttribute
{
    public int MinLines { get; }
    public int MaxLines { get; }

    public ArabicTextAreaAttribute(int minLines = 2, int maxLines = 6)
    {
        MinLines = minLines;
        MaxLines = maxLines;
    }
}

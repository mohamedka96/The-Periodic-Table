using System;
using UnityEngine;

[Serializable]
public class ElementEntry
{
    [Tooltip("الرقم الذري (1-118)")]
    public int atomicNumber;

    [Tooltip("الرمز الكيميائي")]
    public string symbol;

    [Tooltip("الاسم بالعربية")]
    [ArabicTextArea(1, 2)]
    public string nameArabic;

    [Tooltip("الكتلة الذرية")]
    public float atomicMass;

    [Tooltip("التصنيف بالعربية (معدن، غاز، ...)")]
    [ArabicTextArea(1, 2)]
    public string categoryArabic;

    [ArabicTextArea(3, 8)]
    [Tooltip("وصف تعليمي قابل للتعديل من Inspector")]
    public string descriptionArabic;

    public int Protons => atomicNumber;
    public int Electrons => atomicNumber;
    public int Neutrons => Mathf.Max(0, Mathf.RoundToInt(atomicMass) - atomicNumber);

    public bool IsValid => atomicNumber >= 1 && atomicNumber <= 118;
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DisplaySettings))]
public class DisplaySettingsEditor : Editor
{
    static ElementEntry PreviewSample => new ElementEntry
    {
        atomicNumber = 6,
        symbol = "C",
        nameArabic = "الكربون",
        atomicMass = 12.011f,
        categoryArabic = "لا فلز",
        descriptionArabic = "عنصر الحياة الأساسي — مثال للمعاينة."
    };

    public override void OnInspectorGUI()
    {
        var settings = (DisplaySettings)target;

        EditorGUILayout.HelpBox(
            "عدّل القوالب في الحقول. «معاينة القالب» تُظهر شكل النص في اللعبة (عربي + أرقام + رموز).",
            MessageType.Info);

        Undo.RecordObject(settings, "Edit Display Settings");

        settings.showTitle = EditorGUILayout.Toggle("إظهار العنوان", settings.showTitle);
        settings.showFacts = EditorGUILayout.Toggle("إظهار الحقائق", settings.showFacts);
        settings.showDescription = EditorGUILayout.Toggle("إظهار الوصف", settings.showDescription);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("قوالب النص", EditorStyles.boldLabel);

        settings.titleTemplate = ArabicInspectorUtility.DrawArabicField(
            "قالب العنوان", settings.titleTemplate, 2, 4);
        DrawTemplatePreview("معاينة العنوان", settings.titleTemplate);

        settings.factsTemplate = ArabicInspectorUtility.DrawArabicField(
            "قالب الحقائق", settings.factsTemplate, 4, 10);
        DrawTemplatePreview("معاينة الحقائق", settings.factsTemplate);

        settings.descriptionTemplate = ArabicInspectorUtility.DrawArabicField(
            "قالب الوصف", settings.descriptionTemplate, 2, 4);
        DrawTemplatePreview("معاينة الوصف", settings.descriptionTemplate);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("تنسيق النص", EditorStyles.boldLabel);
        settings.titleFontSize = EditorGUILayout.FloatField("حجم العنوان", settings.titleFontSize);
        settings.titleColor = EditorGUILayout.ColorField("لون العنوان", settings.titleColor);
        settings.bodyFontSize = EditorGUILayout.FloatField("حجم المتن", settings.bodyFontSize);
        settings.bodyColor = EditorGUILayout.ColorField("لون المتن", settings.bodyColor);

        if (GUI.changed)
            EditorUtility.SetDirty(settings);
    }

    static void DrawTemplatePreview(string label, string template)
    {
        if (string.IsNullOrWhiteSpace(template)) return;
        string filled = DisplaySettings.FormatTemplate(template, PreviewSample);
        ArabicInspectorUtility.DrawPreviewBox(label, filled);
    }
}

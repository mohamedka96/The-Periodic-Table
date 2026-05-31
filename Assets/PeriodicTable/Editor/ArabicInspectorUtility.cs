using System;
using ArabicSupport;
using UnityEditor;
using UnityEngine;

/// <summary>
/// أدوات عرض وتعديل النص العربي في Inspector بشكل مقروء.
/// </summary>
public static class ArabicInspectorUtility
{
    public static string ToDisplay(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
        return ArabicFixer.Fix(raw, true, true);
    }

    static GUIStyle PreviewStyle
    {
        get
        {
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                wordWrap = true,
                fontSize = 14,
                alignment = TextAnchor.UpperRight,
                padding = new RectOffset(12, 12, 10, 10)
            };
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.82f, 1f, 0.82f)
                : new Color(0.05f, 0.4f, 0.08f);
            return style;
        }
    }

    static GUIStyle LogicalEditStyle => new GUIStyle(EditorStyles.textArea) { wordWrap = true, fontSize = 12 };

    public static string DrawArabicField(string label, string value, int minLines = 1, int maxLines = 6)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        string display = ToDisplay(value);
        float previewHeight = EditorGUIUtility.singleLineHeight * Mathf.Clamp(
            CountLines(display, minLines), minLines, maxLines);

        EditorGUILayout.LabelField("▼ عربي مقروء (كما في اللعبة):", EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(display, PreviewStyle, GUILayout.MinHeight(previewHeight));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("✎ محرر عربي", GUILayout.Width(110)))
            ArabicTextEditorWindow.Open(value, updated => value = updated);
        if (GUILayout.Button("نسخ المعاينة", GUILayout.Width(100)))
            EditorGUIUtility.systemCopyBuffer = display;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        return value;
    }

    public static void DrawPreviewBox(string label, string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return;
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(ToDisplay(rawText), PreviewStyle, GUILayout.MinHeight(44));
        EditorGUILayout.Space(4);
    }

    public static void DrawArabicProperty(SerializedProperty property, GUIContent label, int minLines, int maxLines)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUILayout.PropertyField(property, label);
            return;
        }

        EditorGUILayout.LabelField(label.text, EditorStyles.boldLabel);

        string display = ToDisplay(property.stringValue);
        float previewHeight = EditorGUIUtility.singleLineHeight * Mathf.Clamp(
            CountLines(display, minLines), minLines, maxLines);

        EditorGUILayout.LabelField("▼ عربي مقروء:", EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(display, PreviewStyle, GUILayout.MinHeight(previewHeight));

        if (GUILayout.Button("✎ محرر عربي"))
        {
            ArabicTextEditorWindow.Open(property.stringValue, v =>
            {
                property.stringValue = v;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        EditorGUILayout.Space(4);
    }

    static int CountLines(string text, int fallback)
    {
        if (string.IsNullOrEmpty(text)) return fallback;
        return Mathf.Max(fallback, text.Split('\n').Length + 1);
    }
}

[CustomPropertyDrawer(typeof(ArabicTextAreaAttribute))]
public class ArabicTextAreaDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var attr = (ArabicTextAreaAttribute)attribute;
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float y = position.y;
        float w = position.width;

        var labelRect = new Rect(position.x, y, w, line);
        EditorGUI.LabelField(labelRect, label.text, EditorStyles.boldLabel);
        y = labelRect.yMax + spacing;

        string display = ArabicInspectorUtility.ToDisplay(property.stringValue);
        int lineCount = Mathf.Clamp(
            string.IsNullOrEmpty(display) ? attr.MinLines : display.Split('\n').Length + 1,
            attr.MinLines, attr.MaxLines);

        var hintRect = new Rect(position.x, y, w, line);
        EditorGUI.LabelField(hintRect, "عربي مقروء:", EditorStyles.miniLabel);
        y = hintRect.yMax + spacing;

        float previewH = line * lineCount * 1.35f;
        var previewRect = new Rect(position.x, y, w, previewH);
        var previewStyle = new GUIStyle(EditorStyles.helpBox)
        {
            wordWrap = true,
            alignment = TextAnchor.UpperRight,
            fontSize = 12
        };
        previewStyle.normal.textColor = EditorGUIUtility.isProSkin
            ? new Color(0.82f, 1f, 0.82f)
            : new Color(0.05f, 0.4f, 0.08f);

        EditorGUI.SelectableLabel(previewRect, display, previewStyle);
        y = previewRect.yMax + spacing;

        var btnRect = new Rect(position.x, y, 90, line);
        if (GUI.Button(btnRect, "✎ محرر"))
        {
            ArabicTextEditorWindow.Open(property.stringValue, v =>
            {
                property.stringValue = v;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var attr = (ArabicTextAreaAttribute)attribute;
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        string display = ArabicInspectorUtility.ToDisplay(property.stringValue);
        int lineCount = Mathf.Clamp(
            string.IsNullOrEmpty(display) ? attr.MinLines : display.Split('\n').Length + 1,
            attr.MinLines, attr.MaxLines);

        return line + spacing + line + spacing + line * lineCount * 1.35f + spacing + line + spacing;
    }
}

public class ArabicTextEditorWindow : EditorWindow
{
    string logicalText = "";
    string previewText = "";
    Action<string> onApply;
    Vector2 scroll;

    public static void Open(string currentValue, Action<string> applyCallback)
    {
        var window = CreateInstance<ArabicTextEditorWindow>();
        window.titleContent = new GUIContent("محرر عربي");
        window.logicalText = currentValue ?? "";
        window.onApply = applyCallback;
        window.minSize = new Vector2(500, 400);
        window.UpdatePreview();
        window.ShowUtility();
    }

    [MenuItem("Tools/Arabic Text Editor")]
    public static void OpenStandalone() => Open("", _ => { });

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "اكتب في الأسفل بشكل عادي. المعاينة العلوية تُظهر النص كما في اللعبة.",
            MessageType.Info);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("معاينة — كما في اللعبة", EditorStyles.boldLabel);
        var previewStyle = new GUIStyle(EditorStyles.helpBox)
        {
            wordWrap = true,
            fontSize = 16,
            alignment = TextAnchor.UpperRight,
            padding = new RectOffset(12, 12, 10, 10)
        };
        previewStyle.normal.textColor = EditorGUIUtility.isProSkin
            ? new Color(0.8f, 1f, 0.8f)
            : new Color(0.05f, 0.45f, 0.08f);

        EditorGUILayout.SelectableLabel(previewText, previewStyle, GUILayout.MinHeight(100));

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("تعديل النص", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        logicalText = EditorGUILayout.TextArea(logicalText, GUILayout.MinHeight(120));
        if (EditorGUI.EndChangeCheck())
            UpdatePreview();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("نسخ المعاينة"))
            EditorGUIUtility.systemCopyBuffer = previewText;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("إلغاء", GUILayout.Width(80))) Close();
        if (GUILayout.Button("تطبيق", GUILayout.Width(80)))
        {
            onApply?.Invoke(logicalText);
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    void UpdatePreview() => previewText = ArabicInspectorUtility.ToDisplay(logicalText);
}

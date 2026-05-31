using System.IO;
using UnityEditor;
using UnityEngine;

public static class PeriodicTableDatabaseBuilder
{
    const string DatabasePath = "Assets/PeriodicTable/Data/PeriodicTableDatabase.asset";
    const string DisplaySettingsPath = "Assets/PeriodicTable/Data/DisplaySettings.asset";

    [MenuItem("Periodic Table/Create Or Refresh Database")]
    public static void CreateOrRefreshDatabase()
    {
        EnsureFolder("Assets/PeriodicTable/Data");

        var database = LoadOrCreate<PeriodicTableDatabase>(DatabasePath);
        database.elements = PeriodicTableDataFactory.CreateDefaultElements();
        EditorUtility.SetDirty(database);

        var display = LoadOrCreate<DisplaySettings>(DisplaySettingsPath);
        EditorUtility.SetDirty(display);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var manager = Object.FindFirstObjectByType<TableManager>();
        if (manager != null)
        {
            Undo.RecordObject(manager, "Assign Database");
            manager.database = database;
            manager.displaySettings = display;
            EditorUtility.SetDirty(manager);
        }

        EditorUtility.DisplayDialog(
            "Periodic Table",
            "تم إنشاء/تحديث قاعدة البيانات.\n\n" +
            "• عدّل العناصر: Periodic Table > Edit Elements\n" +
            "• عدّل قالب العرض: DisplaySettings.asset",
            "OK");

        Selection.activeObject = database;
    }

    [MenuItem("Periodic Table/Edit Elements")]
    public static void OpenElementEditor()
    {
        ElementDatabaseEditorWindow.ShowWindow();
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}

[CustomEditor(typeof(PeriodicTableDatabase))]
public class PeriodicTableDatabaseEditor : Editor
{
    int selectedNumber = 1;
    string search = "";

    public override void OnInspectorGUI()
    {
        var db = (PeriodicTableDatabase)target;

        EditorGUILayout.HelpBox(
            "عدّل العناصر من هنا مباشرة، أو افتح: Periodic Table > Edit Elements\n" +
            "حقل التعديل قد يبدو معكوساً — المعاينة تُظهر شكل النص في اللعبة.",
            MessageType.Info);

        if (GUILayout.Button("Periodic Table > Edit Elements (نافذة كبيرة)"))
            ElementDatabaseEditorWindow.ShowWindow();

        if (GUILayout.Button("تحديث / استعادة البيانات الافتراضية"))
        {
            Undo.RecordObject(db, "Refresh Database");
            db.elements = PeriodicTableDataFactory.CreateDefaultElements();
            EditorUtility.SetDirty(db);
        }

        EditorGUILayout.Space(6);
        search = EditorGUILayout.TextField("بحث", search);

        if (string.IsNullOrWhiteSpace(search))
            selectedNumber = EditorGUILayout.IntSlider("الرقم الذري", selectedNumber, 1, 118);
        else
            DrawSearchResults(db);

        ElementDatabaseEditorWindow.DrawElementFields(db, db.GetElement(selectedNumber));
    }

    void DrawSearchResults(PeriodicTableDatabase db)
    {
        foreach (var entry in db.elements)
        {
            if (entry == null || !ElementDatabaseEditorWindow.MatchesSearch(entry, search)) continue;

            if (GUILayout.Button($"{entry.atomicNumber} — {entry.symbol}"))
                selectedNumber = entry.atomicNumber;
        }
    }
}

public class ElementDatabaseEditorWindow : EditorWindow
{
    PeriodicTableDatabase database;
    int selectedNumber = 1;
    string search = "";
    Vector2 scroll;

    public static void ShowWindow()
    {
        var window = GetWindow<ElementDatabaseEditorWindow>("Edit Elements");
        window.minSize = new Vector2(420, 520);
        window.Show();
    }

    void OnEnable()
    {
        database = AssetDatabase.LoadAssetAtPath<PeriodicTableDatabase>(
            "Assets/PeriodicTable/Data/PeriodicTableDatabase.asset");
    }

    void OnGUI()
    {
        database = (PeriodicTableDatabase)EditorGUILayout.ObjectField(
            "Database", database, typeof(PeriodicTableDatabase), false);

        if (database == null)
        {
            EditorGUILayout.HelpBox("عيّن PeriodicTableDatabase أو أنشئه من Periodic Table > Create Or Refresh Database", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox(
            "المعاينة الخضراء = شكل النص في اللعبة. عدّل في الحقل الأبيض أسفلها.",
            MessageType.None);

        search = EditorGUILayout.TextField("بحث", search);

        if (string.IsNullOrWhiteSpace(search))
            selectedNumber = EditorGUILayout.IntSlider("الرقم الذري", selectedNumber, 1, 118);
        else
        {
            foreach (var entry in database.elements)
            {
                if (entry == null || !MatchesSearch(entry, search)) continue;
                if (GUILayout.Button($"{entry.atomicNumber} — {entry.symbol}"))
                    selectedNumber = entry.atomicNumber;
            }
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawElementFields(database, database.GetElement(selectedNumber));
        EditorGUILayout.EndScrollView();
    }

    public static bool MatchesSearch(ElementEntry entry, string search)
    {
        string q = search.Trim();
        return entry.atomicNumber.ToString().Contains(q)
            || (entry.symbol != null && entry.symbol.ToLower().Contains(q.ToLower()))
            || (entry.nameArabic != null && entry.nameArabic.Contains(q));
    }

    public static void DrawElementFields(PeriodicTableDatabase db, ElementEntry entry)
    {
        if (entry == null)
        {
            EditorGUILayout.HelpBox("العنصر غير موجود.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"عنصر {entry.atomicNumber} — {entry.symbol}", EditorStyles.boldLabel);
        Undo.RecordObject(db, "Edit Element");

        entry.symbol = EditorGUILayout.TextField("الرمز", entry.symbol);
        entry.atomicMass = EditorGUILayout.FloatField("الكتلة الذرية", entry.atomicMass);

        entry.nameArabic = ArabicInspectorUtility.DrawArabicField("الاسم", entry.nameArabic, 1, 2);
        entry.categoryArabic = ArabicInspectorUtility.DrawArabicField("التصنيف", entry.categoryArabic, 1, 2);
        entry.descriptionArabic = ArabicInspectorUtility.DrawArabicField("الوصف", entry.descriptionArabic, 4, 10);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.IntField("البروتونات", entry.Protons);
        EditorGUILayout.IntField("الإلكترونات", entry.Electrons);
        EditorGUILayout.IntField("النيوترونات", entry.Neutrons);
        EditorGUI.EndDisabledGroup();

        if (GUI.changed)
            EditorUtility.SetDirty(db);
    }
}

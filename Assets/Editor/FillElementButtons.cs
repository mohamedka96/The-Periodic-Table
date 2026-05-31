using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FillElementButtons : EditorWindow
{
    TableManager tableManager;
    Transform gridParent;

    [MenuItem("Periodic Table/Fill Element Buttons")]
    public static void ShowWindow() => GetWindow<FillElementButtons>("Fill Buttons");

    void OnGUI()
    {
        GUILayout.Label("ربط أزرار الجدول تلقائيا", EditorStyles.boldLabel);
        tableManager = (TableManager)EditorGUILayout.ObjectField("Table Manager", tableManager, typeof(TableManager), true);
        gridParent = (Transform)EditorGUILayout.ObjectField("Grid Parent", gridParent, typeof(Transform), true);

        if (GUILayout.Button("Fill Buttons"))
        {
            if (tableManager == null || gridParent == null)
            {
                EditorUtility.DisplayDialog("خطأ", "عيّن Table Manager و Grid Parent", "OK");
                return;
            }
            FillButtons();
        }
    }

    void FillButtons()
    {
        Button[] buttons = gridParent.GetComponentsInChildren<Button>(true);
        int count = 0;

        foreach (Button btn in buttons)
        {
            var match = System.Text.RegularExpressions.Regex.Match(btn.gameObject.name, @"\d+");
            if (!match.Success || !int.TryParse(match.Value, out int id)) continue;

            Undo.RecordObject(btn, "Fill Element Button");

            var serialized = new SerializedObject(btn);
            serialized.FindProperty("m_OnClick.m_PersistentCalls").ClearArray();
            serialized.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(
                btn.onClick, tableManager.SelectElement, id);

            EditorUtility.SetDirty(btn);
            count++;
        }

        EditorUtility.DisplayDialog("تم", $"تم ربط {count} زر", "OK");
    }
}

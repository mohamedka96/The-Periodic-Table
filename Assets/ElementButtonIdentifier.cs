using UnityEngine;

/// <summary>
/// يُربط بزر عنصر في الجدول الدوري.
/// </summary>
public class ElementButtonIdentifier : MonoBehaviour
{
    public int elementID;
    public TableManager tableManager;

    public void OnButtonClick()
    {
        if (tableManager != null)
            tableManager.SelectElement(elementID);
    }
}

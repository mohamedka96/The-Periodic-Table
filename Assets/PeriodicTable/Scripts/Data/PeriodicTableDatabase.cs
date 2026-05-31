using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PeriodicTableDatabase", menuName = "Periodic Table/Database")]
public class PeriodicTableDatabase : ScriptableObject
{
    [HideInInspector]
    public List<ElementEntry> elements = new List<ElementEntry>();

    public ElementEntry GetElement(int atomicNumber)
    {
        if (atomicNumber < 1 || atomicNumber > 118) return null;

        foreach (var entry in elements)
        {
            if (entry != null && entry.atomicNumber == atomicNumber)
                return entry;
        }

        return null;
    }

    public void EnsureDefaults()
    {
        if (elements != null && elements.Count >= 118) return;
        elements = PeriodicTableDataFactory.CreateDefaultElements();
    }
}

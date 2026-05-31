using UnityEngine;
using TMPro;
using ArabicSupport;

/// <summary>
/// شاشة عرض واحدة: نص العنصر + نموذج الذرة + التحكم بها.
/// </summary>
[System.Serializable]
public class DisplaySlot
{
    [Header("واجهة العرض")]
    public GameObject infoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    [Header("حاوية الذرة")]
    public Transform atomContainer;

    AtomVisualizer visualizer;
    float currentScale = 1f;

    public void Initialize(float electronSpeed, float scale, GameObject proton, GameObject neutron, GameObject electron)
    {
        if (atomContainer == null) return;

        visualizer = atomContainer.GetComponent<AtomVisualizer>();
        if (visualizer == null)
            visualizer = atomContainer.gameObject.AddComponent<AtomVisualizer>();

        visualizer.protonPrefab = proton;
        visualizer.neutronPrefab = neutron;
        visualizer.electronPrefab = electron;
        visualizer.electronSpeed = electronSpeed;

        currentScale = scale;
        visualizer.ClearAtom();
        SetAtomScale(scale);
    }

    public void ShowElement(ElementEntry element, DisplaySettings settings)
    {
        if (atomContainer != null)
            atomContainer.gameObject.SetActive(true);

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (settings != null)
        {
            settings.ApplyTo(element, out string title, out string body);
            SetText(titleText, title, settings.titleFontSize, settings.titleColor);
            SetText(bodyText, body, settings.bodyFontSize, settings.bodyColor);
        }

        visualizer?.BuildAtom(element);
        visualizer?.SetScale(currentScale);
    }

    static void SetText(TextMeshProUGUI textField, string content, float fontSize, Color color)
    {
        if (textField == null) return;
        textField.text = ArabicFixer.Fix(content, true, true);
        textField.fontSize = fontSize;
        textField.color = color;
    }

    public void SetAtomScale(float scale)
    {
        currentScale = scale;
        visualizer?.SetScale(scale);
    }

    public void SetElectronSpeed(float speed)
    {
        if (visualizer != null)
            visualizer.ElectronSpeed = speed;
    }

    public void ToggleRotation()
    {
        visualizer?.ToggleRotation();
    }

    public void Close()
    {
        visualizer?.ClearAtom();

        if (atomContainer != null)
            atomContainer.gameObject.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}

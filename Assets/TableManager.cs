using UnityEngine;
using TMPro;

/// <summary>
/// المدير الرئيسي — يتناوب بين شاشتين عند كل ضغطة على عنصر.
/// </summary>
public class TableManager : MonoBehaviour
{
    [Header("قاعدة البيانات")]
    public PeriodicTableDatabase database;
    public DisplaySettings displaySettings;

    [Header("الشاشة 1")]
    public GameObject[] infoPanels;
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] detailsTexts;
    public Transform atomContainerSlot1;

    [Header("الشاشة 2 — يُملأ تلقائيا إذا تُرك فارغا")]
    public Transform atomContainerSlot2;

    [Header("Prefabs الذرة")]
    public GameObject protonPrefab;
    public GameObject neutronPrefab;
    public GameObject electronPrefab;

    [Header("إعدادات افتراضية")]
    public float electronRotateSpeed = 100f;
    public float defaultAtomScale = 1f;

    [Header("تنسيق النص — يُطبّق على قالب العرض")]
    public float nameFontSize = 0.1f;
    public Color nameColor = Color.white;
    public float detailsFontSize = 0.1f;
    public Color detailsColor = Color.white;

    DisplaySlot slot1 = new DisplaySlot();
    DisplaySlot slot2 = new DisplaySlot();
    bool useSlot1Next = true;

    void Awake()
    {
        EnsureDatabase();
        EnsureDisplaySettings();
        BindSlots();
    }

    void EnsureDatabase()
    {
        if (database != null)
        {
            database.EnsureDefaults();
            return;
        }

        database = ScriptableObject.CreateInstance<PeriodicTableDatabase>();
        database.EnsureDefaults();
        Debug.LogWarning("TableManager: أنشئ قاعدة البيانات من القائمة Periodic Table > Create Or Refresh Database");
    }

    void EnsureDisplaySettings()
    {
        if (displaySettings == null)
            displaySettings = ScriptableObject.CreateInstance<DisplaySettings>();

        displaySettings.titleFontSize = nameFontSize;
        displaySettings.titleColor = nameColor;
        displaySettings.bodyFontSize = detailsFontSize;
        displaySettings.bodyColor = detailsColor;
    }

    void BindSlots()
    {
        slot1.infoPanel = infoPanels != null && infoPanels.Length > 0 ? infoPanels[0] : null;
        slot1.titleText = nameTexts != null && nameTexts.Length > 0 ? nameTexts[0] : null;
        slot1.bodyText = detailsTexts != null && detailsTexts.Length > 0 ? detailsTexts[0] : null;
        slot1.atomContainer = atomContainerSlot1;
        slot1.Initialize(electronRotateSpeed, defaultAtomScale, protonPrefab, neutronPrefab, electronPrefab);

        slot2.infoPanel = infoPanels != null && infoPanels.Length > 1 ? infoPanels[1] : null;
        slot2.titleText = nameTexts != null && nameTexts.Length > 1 ? nameTexts[1] : null;
        slot2.bodyText = detailsTexts != null && detailsTexts.Length > 1 ? detailsTexts[1] : null;
        slot2.atomContainer = atomContainerSlot2;
        slot2.Initialize(electronRotateSpeed, defaultAtomScale, protonPrefab, neutronPrefab, electronPrefab);
    }

    /// <summary>يُستدعى من أزرار الجدول — يتناوب بين الشاشتين.</summary>
    public void SelectElement(int atomicNumber)
    {
        DisplaySlot target = useSlot1Next ? slot1 : slot2;
        useSlot1Next = !useSlot1Next;

        ElementEntry element = database.GetElement(atomicNumber);
        if (element == null)
        {
            Debug.LogWarning($"TableManager: عنصر غير موجود — {atomicNumber}");
            return;
        }

        target.ShowElement(element, displaySettings);
    }

    // ── تحكم الشاشة 1 (مربوط بالـ UI) ──
    public void AdjustAtomScaleSlot1(float scale) => slot1.SetAtomScale(scale);
    public void SetElectronSpeedSlot1(float speed) => slot1.SetElectronSpeed(speed);
    public void ToggleRotationSlot0() => slot1.ToggleRotation();
    public void ClosePanelSlot0() => slot1.Close();

    // ── تحكم الشاشة 2 ──
    public void AdjustAtomScaleSlot2(float scale) => slot2.SetAtomScale(scale);
    public void SetElectronSpeedSlot2(float speed) => slot2.SetElectronSpeed(speed);
    public void ToggleRotationSlot1() => slot2.ToggleRotation();
    public void ClosePanelSlot1() => slot2.Close();
}

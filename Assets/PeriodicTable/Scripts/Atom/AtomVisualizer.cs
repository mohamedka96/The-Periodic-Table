using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// يبني نموذج الذرة ثلاثي الأبعاد بأعداد حقيقية للجسيمات.
/// </summary>
public class AtomVisualizer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject protonPrefab;
    public GameObject neutronPrefab;
    public GameObject electronPrefab;

    [Header("إعدادات الحركة")]
    public float electronSpeed = 100f;
    public bool electronsRotating = true;

    [Header("أداء — للعناصر الثقيلة")]
    [Tooltip("الحد الأقصى لجسيمات النواة المعروضة (الأعداد الحقيقية تظهر في النص)")]
    public int maxNucleusParticles = 120;

    static readonly Color[] ShellColors =
    {
        Color.cyan, Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red, Color.magenta, Color.blue
    };

    readonly List<VRAtomRotator> electronRotators = new List<VRAtomRotator>();
    Transform nucleusTransform;
    float currentScale = 1f;

    public float ElectronSpeed
    {
        get => electronSpeed;
        set
        {
            electronSpeed = value;
            ApplySpeedToElectrons();
        }
    }

    public bool ElectronsRotating
    {
        get => electronsRotating;
        set
        {
            electronsRotating = value;
            ApplyRotationState();
        }
    }

    public void SetScale(float scale)
    {
        currentScale = scale;
        transform.localScale = Vector3.one * scale;
    }

    public void ToggleRotation()
    {
        ElectronsRotating = !electronsRotating;
    }

    public void BuildAtom(ElementEntry element)
    {
        ClearAtom();
        if (element == null || !element.IsValid) return;

        nucleusTransform = new GameObject("Nucleus").transform;
        nucleusTransform.SetParent(transform, false);
        nucleusTransform.localPosition = Vector3.zero;

        SpawnNucleus(element.Protons, element.Neutrons);
        SpawnElectrons(element.Electrons);
    }

    void SpawnNucleus(int protons, int neutrons)
    {
        int total = protons + neutrons;
        int visualProtons = protons;
        int visualNeutrons = neutrons;

        if (total > maxNucleusParticles)
        {
            float ratio = (float)maxNucleusParticles / total;
            visualProtons = Mathf.Max(1, Mathf.RoundToInt(protons * ratio));
            visualNeutrons = Mathf.Max(0, maxNucleusParticles - visualProtons);
        }

        float spread = 0.08f + (visualProtons + visualNeutrons) * 0.002f;
        SpawnParticles(protonPrefab, visualProtons, spread);
        SpawnParticles(neutronPrefab, visualNeutrons, spread);
    }

    void SpawnParticles(GameObject prefab, int count, float spread)
    {
        if (prefab == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var particle = Instantiate(prefab, nucleusTransform, false);
            particle.transform.localPosition = Random.insideUnitSphere * spread;
            particle.transform.localRotation = Quaternion.identity;
        }
    }

    void SpawnElectrons(int electronCount)
    {
        if (electronPrefab == null || electronCount <= 0) return;

        int[] shells = AtomicDataUtility.GetElectronShells(electronCount);
        int shellIndex = 1;

        foreach (int countInShell in shells)
        {
            float radius = 0.35f + shellIndex * 0.22f;
            Vector3 axis = Random.onUnitSphere;
            Color color = ShellColors[(shellIndex - 1) % ShellColors.Length];
            float speed = electronSpeed * (1.5f / shellIndex);

            for (int i = 0; i < countInShell; i++)
            {
                GameObject electron = Instantiate(electronPrefab, transform, false);
                electron.transform.localPosition = Vector3.zero;
                electron.transform.localRotation = Quaternion.identity;
                electron.name = $"Electron_S{shellIndex}_{i}";

                VRAtomRotator rotator = electron.GetComponent<VRAtomRotator>();
                if (rotator == null) rotator = electron.AddComponent<VRAtomRotator>();

                rotator.InitializeOrbit(nucleusTransform, axis, speed, electronsRotating, color, radius, shellIndex);
                electronRotators.Add(rotator);
            }

            shellIndex++;
        }
    }

    void ApplySpeedToElectrons()
    {
        foreach (var rotator in electronRotators)
        {
            if (rotator == null) continue;
            rotator.speed = electronSpeed * (1.5f / rotator.shellIndex);
            rotator.isRotating = electronsRotating;
        }
    }

    void ApplyRotationState()
    {
        foreach (var rotator in electronRotators)
        {
            if (rotator != null)
                rotator.isRotating = electronsRotating;
        }
    }

    public void ClearAtom()
    {
        electronRotators.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        nucleusTransform = null;
    }

    void OnDestroy() => ClearAtom();
}

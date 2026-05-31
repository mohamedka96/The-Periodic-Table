using UnityEngine;

public class VRAtomRotator : MonoBehaviour
{
    [HideInInspector] public Transform centerPoint;
    [HideInInspector] public float speed = 50f;
    [HideInInspector] public Vector3 axis = Vector3.up;
    [HideInInspector] public bool isRotating = true;
    [HideInInspector] public int shellIndex = 1;

    Color assignedColor;
    float initialRadius;
    LineRenderer lineRenderer;
    bool isInitialized;
    float currentAngle;

    Transform OrbitRoot => centerPoint != null && centerPoint.parent != null
        ? centerPoint.parent
        : transform.parent;

    public void InitializeOrbit(Transform center, Vector3 rotationAxis, float rotationSpeed, bool rotationState, Color color, float radius, int shell = 1)
    {
        centerPoint = center;
        axis = rotationAxis.normalized;
        speed = rotationSpeed;
        isRotating = rotationState;
        assignedColor = color;
        initialRadius = radius;
        shellIndex = shell;
        currentAngle = Random.Range(0f, 360f);

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = assignedColor;
        lineRenderer.endColor = assignedColor;

        isInitialized = true;
        UpdateElectronAndOrbit();
    }

    void Update()
    {
        if (!isInitialized || centerPoint == null) return;

        if (isRotating)
        {
            currentAngle += speed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;
        }

        UpdateElectronAndOrbit();
    }

    void UpdateElectronAndOrbit()
    {
        if (lineRenderer == null || centerPoint == null) return;

        const int segments = 60;
        lineRenderer.positionCount = segments + 1;

        Transform root = OrbitRoot;
        float visualScale = root != null ? root.lossyScale.x : 1f;
        lineRenderer.startWidth = 0.012f * visualScale;
        lineRenderer.endWidth = 0.012f * visualScale;

        Quaternion rotationOffset = Quaternion.FromToRotation(Vector3.up, axis);

        // نصف القطر في الإحداثيات المحلية فقط — التكبير يتم عبر localScale للحاوية
        float electronRad = currentAngle * Mathf.Deg2Rad;
        Vector3 localPos = centerPoint.localPosition + rotationOffset * new Vector3(
            Mathf.Cos(electronRad) * initialRadius, 0, Mathf.Sin(electronRad) * initialRadius);

        transform.localPosition = localPos;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
            Vector3 localPoint = centerPoint.localPosition + rotationOffset * new Vector3(
                Mathf.Cos(angle) * initialRadius, 0, Mathf.Sin(angle) * initialRadius);

            Vector3 worldPoint = root != null ? root.TransformPoint(localPoint) : localPoint;
            lineRenderer.SetPosition(i, worldPoint);
        }
    }
}

using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform vrCameraTransform;

    void Start()
    {
        // البحث عن الكاميرا الأساسية في المشهد تلقائياً عند بدء اللعبة
        if (Camera.main != null)
        {
            vrCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("لم يتم العثور على كاميرا أساسية (Main Camera) في المشهد! تأكد من تحديد نوع الكاميرا كـ MainCamera.");
        }
    }

    void LateUpdate()
    {
        if (vrCameraTransform != null)
        {
            // حساب الاتجاه نحو الكاميرا
            Vector3 targetDirection = vrCameraTransform.position - transform.position;

            // تصفير المحور العمودي (Y) لضمان أن الشاشة تتحرك يميناً ويساراً فقط، ولا تميل للأعلى أو الأسفل
            targetDirection.y = 0;

            // إذا تحركت الكاميرا، قم بتدوير الشاشة لتواجهها تماماً
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-targetDirection); 
                transform.rotation = targetRotation;
            }
        }
    }
}
using UnityEngine;

public class ForceAspectRatio : MonoBehaviour
{
    // 목표 비율 9:16 (0.5625)
    private float targetAspectRatio = 9f / 16f;

    void Start()
    {
        UpdateAspectRatio();
    }

    // Resizable Window 대응을 위해 Update 또는 OnPreRender에서 체크
    void Update()
    {
        UpdateAspectRatio();
    }

    void UpdateAspectRatio()
    {
        // 현재 창의 해상도 가져오기
        float windowAspect = (float)Screen.width / (float)Screen.height;
        // 현재 비율을 목표 비율로 나눈 값
        float scaleHeight = windowAspect / targetAspectRatio;

        Camera camera = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            // 창이 목표보다 세로로 더 긴 경우 (위아래 레터박스)
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            // 창이 목표보다 가로로 더 긴 경우 (좌우 필러박스)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}
using UnityEngine;

/// <summary>
/// Trauma-based screen shake applied to the main camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    Camera cam;
    Vector3 basePos;
    float trauma;
    float time;

    public void Init(Camera camera)
    {
        cam = camera;
        basePos = cam.transform.position;
    }

    public void AddTrauma(float t)
    {
        trauma = Mathf.Clamp01(trauma + t);
    }

    void LateUpdate()
    {
        if (cam == null) return;
        time += Time.deltaTime;
        trauma = Mathf.Max(0f, trauma - Time.deltaTime * 1.4f);
        float mag = trauma * trauma * 0.45f;
        float ox = (Mathf.Sin(time * 61f) + Mathf.Sin(time * 37f) * 0.6f) * 0.5f * mag;
        float oy = (Mathf.Cos(time * 53f) + Mathf.Sin(time * 43f) * 0.6f) * 0.5f * mag;
        cam.transform.position = basePos + new Vector3(ox, oy, 0f);
    }
}

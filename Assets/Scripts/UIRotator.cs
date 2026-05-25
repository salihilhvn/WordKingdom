using UnityEngine;

public class UIRotator : MonoBehaviour
{
    [Tooltip("Saniyede kaç derece dönecek")]
    public float rotationSpeed = 90f;
    
    [Tooltip("Saat yönünde mi dönecek?")]
    public bool clockwise = false;

    private void Update()
    {
        float dir = clockwise ? -1f : 1f;
        // unscaledDeltaTime kullanıyoruz ki Time.timeScale 0 olsa bile (oyun dursa bile) dönsün
        transform.Rotate(0f, 0f, rotationSpeed * dir * Time.unscaledDeltaTime, Space.Self);
    }
}

using UnityEngine;
using UnityEditor;

public class PlayerPrefsClearer : MonoBehaviour
{
    [MenuItem("Word Kingdom/Sıfırla/Tüm Kayıtları (PlayerPrefs) Sil")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Tüm seviyeler ve kayıtlar başarıyla sıfırlandı!");
    }
}

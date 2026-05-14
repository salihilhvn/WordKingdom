using UnityEngine;
using TMPro;

public class LetterTile : MonoBehaviour
{
    [Header("Grid Position")]
    public int x;
    public int y;

    [Header("Data")]
    public char letter;
    public bool isSolved = false;

    [Header("References")]
    [Tooltip("Prefab içindeki Canvas altındaki TextMeshPro objesi")]
    public TMP_Text letterText;
    [Tooltip("Prefab içindeki button objesinin MeshRenderer'ı")]
    public MeshRenderer buttonRenderer;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.green;
    public Color solvedColor = Color.yellow;

    private void Start()
    {
        // Başlangıç rengini ayarla
        SetColor(defaultColor);
    }

    public void SetData(int posX, int posY, char c)
    {
        x = posX;
        y = posY;
        letter = c;
        if (letterText != null)
        {
            letterText.text = letter.ToString();
        }
    }

    public void Select()
    {
        if (isSolved) return;
        SetColor(selectedColor);
    }

    public void Deselect()
    {
        if (isSolved) return;
        SetColor(defaultColor);
    }

    public void SetSolved()
    {
        isSolved = true;
        SetColor(solvedColor);
    }

    private void SetColor(Color newColor)
    {
        if (buttonRenderer != null)
        {
            // Standart materyaller için
            buttonRenderer.material.color = newColor;
            
            // Eğer URP kullanıyorsan (Lit material gibi) asıl renk _BaseColor'dır
            if (buttonRenderer.material.HasProperty("_BaseColor"))
            {
                buttonRenderer.material.SetColor("_BaseColor", newColor);
            }
        }
        else
        {
            Debug.LogError("HATA: " + gameObject.name + " objesinde 'Button Renderer' boş! Lütfen Prefab'e gidip Inspector'dan 'button' objesini bu alana sürükle.");
        }
    }
}

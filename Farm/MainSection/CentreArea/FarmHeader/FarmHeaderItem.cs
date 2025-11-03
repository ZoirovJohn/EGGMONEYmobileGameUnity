using UnityEngine;
using UnityEngine.UI;
using System;

public class FarmHeaderItem : MonoBehaviour
{
    public Button imageBtn;   // Assign your ImageBtn here
    public Image background;  // Optional: for highlight effect
    public Action onClick;    // Called when clicked

    void Awake()
    {
        if (imageBtn != null)
        {
            imageBtn.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetFarm(int id)
    {
        // Optional: you can store ID or update label
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
        {
            Color c = background.color;
            c.a = selected ? 1f : 0.5f;
            background.color = c;
        }
    }
}

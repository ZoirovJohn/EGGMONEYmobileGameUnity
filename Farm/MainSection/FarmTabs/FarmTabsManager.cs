using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmTabsManager : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject farmSlotPrefab;
    public GameObject lockSlotPrefab;
    public GameObject dividerPrefab;
    public Button leftButton;
    public Button rightButton;

    [Header("Settings")]
    public float mainItemWidth = 200f;
    public float dividerWidth = 30f;
    public int startingFarmCount = 3; // unlocked farms
    public int maxFarmCount = 10;     // total farms including locked

    private List<FarmHeaderItem> farmItems = new List<FarmHeaderItem>();
    private int selectedFarmIndex = 0;
    private float viewportWidth;

    void Start()
    {
        viewportWidth = scrollRect.viewport.rect.width;

        leftButton.onClick.AddListener(() => Scroll(-1));
        rightButton.onClick.AddListener(() => Scroll(1));

        BuildItems();
        SelectFarm(0, true);
    }

    private void BuildItems()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        farmItems.Clear();
        float posX = 0f;

        // Initial divider
        CreateDivider(ref posX);

        // Unlocked farms
        for (int i = 0; i < startingFarmCount; i++)
        {
            CreateFarmSlot(i, ref posX);
            CreateDivider(ref posX);
        }

        // Locked farms
        int lockCount = maxFarmCount - startingFarmCount;
        for (int i = 0; i < lockCount; i++)
        {
            CreateLockSlot(i, ref posX);
            CreateDivider(ref posX);
        }

        content.sizeDelta = new Vector2(posX, content.sizeDelta.y);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void CreateFarmSlot(int id, ref float posX)
    {
        GameObject obj = Instantiate(farmSlotPrefab, content);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(posX, 0);
        rt.sizeDelta = new Vector2(mainItemWidth, content.sizeDelta.y);

        FarmHeaderItem item = obj.GetComponent<FarmHeaderItem>();
        item.SetFarm(id);
        item.onClick = () => SelectFarm(id);

        farmItems.Add(item);
        posX += mainItemWidth;
    }

    private void CreateLockSlot(int index, ref float posX)
    {
        GameObject obj = Instantiate(lockSlotPrefab, content);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(posX, 0);
        rt.sizeDelta = new Vector2(mainItemWidth, content.sizeDelta.y);
        posX += mainItemWidth;
    }

    private void CreateDivider(ref float posX)
    {
        GameObject obj = Instantiate(dividerPrefab, content);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(posX, 0);
        rt.sizeDelta = new Vector2(dividerWidth, content.sizeDelta.y);
        posX += dividerWidth;
    }

    private void SelectFarm(int id, bool instant = false)
    {
        selectedFarmIndex = id;
        for (int i = 0; i < farmItems.Count; i++)
            farmItems[i].SetSelected(i == id);

        ScrollToItem(id, instant);
    }

    private void ScrollToItem(int index, bool instant = false)
    {
        float targetX = index * (mainItemWidth + dividerWidth);
        float normalized = Mathf.Clamp01(targetX / (content.sizeDelta.x - viewportWidth));
        if (instant)
            scrollRect.horizontalNormalizedPosition = normalized;
        else
            StartCoroutine(SmoothScroll(normalized));
    }

    private System.Collections.IEnumerator SmoothScroll(float target)
    {
        float duration = 0.25f;
        float start = scrollRect.horizontalNormalizedPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
    }

    private void Scroll(int dir)
    {
        int newIndex = Mathf.Clamp(selectedFarmIndex + dir, 0, farmItems.Count - 1);
        SelectFarm(newIndex);
    }
}

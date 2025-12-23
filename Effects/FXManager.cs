using System.Collections;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [Header("FX Prefabs")]
    [SerializeField] GameObject purchaseFXPrefab;
    [SerializeField] GameObject gameFXPrefab;
    [SerializeField] GameObject touchFXPrefab;

    [Header("FX Parent (world-space empty object)")]
    [SerializeField] Transform fxParent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 🎆 PURCHASE FX (center)
    public void PlayPurchaseFX_Center()
    {
        PlayCenterFX(purchaseFXPrefab);
    }

    // 🎮 GAME FX (single play, center)
    public void PlayGameFX_Center()
    {
        PlayCenterFX(gameFXPrefab);
    }

    // 🎮 GAME FX (play 4 times, then stop)
    public void PlayGameFX_Center_4Times()
    {
        if (!gameFXPrefab || !fxParent) return;
        StartCoroutine(PlayFX4TimesCoroutine(gameFXPrefab, 4));
    }

    // ✨ TOUCH FX (spawn at touch position)
    public void PlayTouchFX(Vector2 screenPos)
    {
        if (!touchFXPrefab || !fxParent) return;

        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane + 5f)
        );

        GameObject fx = Instantiate(touchFXPrefab, worldPos, Quaternion.identity, fxParent);
        fx.transform.localScale *= 0.4f; // make touch FX smaller
    }

    // 🔧 Spawn FX at screen center
    void PlayCenterFX(GameObject prefab)
    {
        if (!prefab || !fxParent) return;

        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 pos = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, cam.nearClipPlane + 5f)
        );

        Instantiate(prefab, pos, Quaternion.identity, fxParent);
    }

    // 🔁 Play FX multiple times (non-looping)
    IEnumerator PlayFX4TimesCoroutine(GameObject prefab, int times)
    {
        Camera cam = Camera.main;
        if (!cam) yield break;

        Vector3 pos = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, cam.nearClipPlane + 5f)
        );

        pos += new Vector3(0f, -7f, 0f);

        GameObject fx = Instantiate(prefab, pos, Quaternion.identity, fxParent);

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (!ps) yield break;

        var main = ps.main;
        main.loop = false; // IMPORTANT

        for (int i = 0; i < times; i++)
        {
            ps.Play(true);
            yield return new WaitForSeconds(main.duration);
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}

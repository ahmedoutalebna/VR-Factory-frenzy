using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class PlatformSettings : MonoBehaviour
{
    [Header("End handle")]
    public GameObject endHandlePrefab;                       // the orange sphere prefab
    public Vector3 defaultOffset = new Vector3(3f, 0f, 0f);  // used only if there is no MovingPlatform script

    [Header("Travel time (seconds)")]
    public float travelTime = 3f;
    public float timeStep = 0.5f;
    public float minTime = 0.5f;
    public float maxTime = 20f;

    // read by the JSON export (VR-FP1-5): the real start, even while the platform is moving
    public Vector3 StartPosition => running ? startPos : transform.position;
    public Vector3 EndPosition => StartPosition + offset;

    Transform handle;
    XRGrabInteractable handleGrab;
    XRGrabInteractable platformGrab;
    Vector3 offset;
    Vector3 startPos;
    LineRenderer line;
    Transform panel;
    TMP_Text timeText;
    MovingPlatform mp;
    bool running = true;

    void Start()
    {
        mp = GetComponent<MovingPlatform>();
        if (mp != null)
        {
            defaultOffset = mp.endOffset;     // start from the values already on the platform
            travelTime = mp.travelTime;
        }
        offset = defaultOffset;
        startPos = transform.position;
        platformGrab = GetComponent<XRGrabInteractable>();

        if (endHandlePrefab == null)
            Debug.LogWarning("PlatformSettings: assign the End Handle Prefab.", this);
        else
        {
            var h = Instantiate(endHandlePrefab, transform.position + offset, Quaternion.identity);
            handle = h.transform;
            handleGrab = h.GetComponent<XRGrabInteractable>();

            line = h.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.05f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = Color.yellow;
        }

        BuildPanel();
        Refresh();
        SetRunning(true);
    }

    void LateUpdate()
    {
        bool grabbed = (platformGrab != null && platformGrab.isSelected) ||
                       (handleGrab != null && handleGrab.isSelected);

        if (grabbed && running) SetRunning(false);          // freeze while you edit
        else if (!grabbed && !running) SetRunning(true);    // let go: restart with the new settings

        if (!running && handle != null)
        {
            if (handleGrab != null && handleGrab.isSelected)
                offset = handle.position - transform.position;      // you are moving the end
            else
                handle.position = transform.position + offset;      // the end follows the platform

            line.SetPosition(0, transform.position);
            line.SetPosition(1, handle.position);
        }

        if (panel != null && Camera.main != null)                   // the panel faces the player
            panel.rotation = Quaternion.LookRotation(panel.position - Camera.main.transform.position);
    }

    void SetRunning(bool value)
    {
        running = value;

        if (running)
        {
            startPos = transform.position;
            if (mp != null)
            {
                mp.endOffset = offset;
                mp.travelTime = travelTime;
                mp.Configure(startPos, startPos + offset, travelTime);
                mp.enabled = true;
            }
        }
        else if (mp != null)
        {
            mp.enabled = false;
            transform.position = startPos;                  // freeze at the start, ready to be moved
        }

        if (handle != null) handle.gameObject.SetActive(!running);   // sphere and line only while editing
    }

    void OnDestroy()
    {
        if (handle != null) Destroy(handle.gameObject);
    }

    // ---------- travel time ----------
    void ChangeTime(float delta)
    {
        travelTime = Mathf.Clamp(travelTime + delta, minTime, maxTime);
        travelTime = Mathf.Round(travelTime / timeStep) * timeStep;
        if (mp != null)
        {
            mp.travelTime = travelTime;
            if (running) mp.Configure(startPos, startPos + offset, travelTime);   // apply immediately
        }
        Refresh();
    }

    void Refresh()
    {
        if (timeText != null) timeText.text = travelTime.ToString("0.0") + " s";
    }

    // ---------- the panel, built in code ----------
    void BuildPanel()
    {
        var rends = GetComponentsInChildren<Renderer>();
        Bounds b = rends.Length > 0 ? rends[0].bounds : new Bounds(transform.position, Vector3.one);
        foreach (var r in rends) b.Encapsulate(r.bounds);

        var go = new GameObject("TimePanel", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(b.center.x, b.max.y + 0.6f, b.center.z);
        go.transform.localScale = Vector3.one * 0.0015f / Mathf.Max(0.0001f, transform.lossyScale.x);
        panel = go.transform;

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        go.AddComponent<TrackedDeviceGraphicRaycaster>();           // lets the XR ray click the buttons
        ((RectTransform)go.transform).sizeDelta = new Vector2(400f, 150f);

        var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(go.transform, false);
        var bgRt = (RectTransform)bg.transform;
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        bgImg.raycastTarget = false;

        timeText = MakeText(go.transform, "", 60, new Vector2(150f, 110f), Vector2.zero);
        MakeButton(go.transform, "-", new Vector2(-140f, 0f), () => ChangeTime(-timeStep));
        MakeButton(go.transform, "+", new Vector2(140f, 0f), () => ChangeTime(timeStep));
    }

    static TMP_Text MakeText(Transform parent, string text, float size, Vector2 rectSize, Vector2 pos)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = rectSize;
        rt.anchoredPosition = pos;
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    static void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn " + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(110f, 110f);
        rt.anchoredPosition = pos;
        go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);
        MakeText(go.transform, label, 90, new Vector2(110f, 110f), Vector2.zero);
    }
}
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class LevelExporter : MonoBehaviour
{
    public float distance = 1.2f;
    public int maxNameLength = 24;

    GameObject root;
    TMP_Text nameText, statusText, exportLabel;
    string levelName = "";
    bool confirmOverwrite;

    static string Folder => Path.Combine(Application.persistentDataPath, "Levels");
    static string PathFor(string n) => Path.Combine(Folder, n + ".json");

    // ---------- open / close (wired to a UI button) ----------
    public void ExportMenu()
    {
        if (root == null) Build();
        bool show = !root.activeSelf;
        root.SetActive(show);
        if (!show) return;

        var head = Camera.main != null ? Camera.main.transform : transform;
        Vector3 fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        root.transform.position = head.position + fwd * distance + Vector3.down * 0.1f;
        root.transform.rotation = Quaternion.LookRotation(root.transform.position - head.position);
        confirmOverwrite = false;
        SetStatus("Type a name, then press Export", Color.white);
        Refresh();
    }

    // ---------- typing ----------
    void AddChar(char c)
    {
        if (levelName.Length >= maxNameLength) return;
        levelName += c;
        confirmOverwrite = false;
        SetStatus("", Color.white);
        Refresh();
    }

    void Backspace()
    {
        if (levelName.Length == 0) return;
        levelName = levelName.Substring(0, levelName.Length - 1);
        confirmOverwrite = false;
        SetStatus("", Color.white);
        Refresh();
    }

    // ---------- export ----------
    void OnExport()
    {
        if (levelName.Length == 0) { SetStatus("Type a name first", Color.red); return; }

        string path = PathFor(levelName);
        if (File.Exists(path) && !confirmOverwrite)
        {
            confirmOverwrite = true;                       // first press: warn, second press: overwrite
            SetStatus("\"" + levelName + ".json\" already exists. Press again to OVERWRITE.", new Color(1f, 0.6f, 0f));
            Refresh();
            return;
        }

        try
        {
            LevelData data = BuildData();
            Directory.CreateDirectory(Folder);
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            Debug.Log("Level exported: " + path);
            SetStatus("Saved " + data.pieces.Count + " elements to " + levelName + ".json", Color.green);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            SetStatus("Export failed: " + e.Message, Color.red);
        }
        confirmOverwrite = false;
        Refresh();
    }

    LevelData BuildData()
    {
        var data = new LevelData { levelName = levelName };

        var mapObj = GameObject.Find("Map");
        Transform anchor = mapObj != null ? mapObj.transform : null;
        if (anchor == null) Debug.LogWarning("No 'Map' object found: positions are exported in world space.");

        Vector3 ToLocal(Vector3 world) => anchor != null ? anchor.InverseTransformPoint(world) : world;
        Vector3 ToLocalEuler(Quaternion world) =>
            (anchor != null ? Quaternion.Inverse(anchor.rotation) * world : world).eulerAngles;

        foreach (var item in FindObjectsOfType<LevelItem>())
        {
            var p = new PieceData
            {
                typeId = item.typeId,
                rotation = ToLocalEuler(item.transform.rotation),
                locked = item.locked
            };

            var ps = item.GetComponent<PlatformSettings>();
            if (ps != null)
            {
                var mp = item.GetComponent<MovingPlatform>();
                p.position = ToLocal(ps.StartPosition);
                p.isMoving = true;
                p.moving.start = ToLocal(ps.StartPosition);
                p.moving.end = ToLocal(ps.EndPosition);
                p.moving.travelTime = ps.travelTime;
                if (mp != null)
                {
                    p.moving.rotationAxis = mp.rotationAxis;
                    p.moving.rotationSpeed = mp.rotationSpeed;
                }
            }
            else p.position = ToLocal(item.transform.position);

            data.pieces.Add(p);
        }
        return data;
    }

    // ---------- UI built in code ----------
    void Refresh()
    {
        nameText.text = levelName.Length == 0 ? "<color=#888888>type a name</color>" : levelName + ".json";
        exportLabel.text = confirmOverwrite ? "Overwrite?" : "Export";
    }

    void SetStatus(string text, Color c) { statusText.text = text; statusText.color = c; }

    void Build()
    {
        root = new GameObject("ExportPanel", typeof(RectTransform));
        root.transform.localScale = Vector3.one * 0.001f;     // 800 x 640 units = 0.8 x 0.64 m

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        root.AddComponent<TrackedDeviceGraphicRaycaster>();   // lets the XR ray click
        ((RectTransform)root.transform).sizeDelta = new Vector2(800f, 640f);

        MakeImage("Bg", Vector2.zero, new Vector2(800f, 640f), new Color(0f, 0f, 0f, 0.85f));
        MakeText("Title", "Export level", 44, new Vector2(700f, 60f), new Vector2(0f, 280f));
        MakeImage("NameBox", new Vector2(0f, 215f), new Vector2(720f, 70f), new Color(0.2f, 0.2f, 0.2f, 1f));
        nameText = MakeText("Name", "", 44, new Vector2(700f, 70f), new Vector2(0f, 215f));

        // AZERTY keyboard, "<" is backspace
        string[] rows = { "1234567890", "AZERTYUIOP", "QSDFGHJKLM", "WXCVBN_-<" };
        float[] ys = { 130f, 64f, -2f, -68f };
        for (int r = 0; r < rows.Length; r++)
        {
            int n = rows[r].Length;
            for (int i = 0; i < n; i++)
            {
                char c = rows[r][i];
                float x = -(n - 1) * 33f + i * 66f;
                if (c == '<') MakeButton("<", new Vector2(x, ys[r]), new Vector2(60f, 60f), new Color(0.6f, 0.3f, 0.3f), Backspace);
                else MakeButton(c.ToString(), new Vector2(x, ys[r]), new Vector2(60f, 60f), new Color(0.2f, 0.4f, 0.8f), () => AddChar(c));
            }
        }

        exportLabel = MakeButton("Export", new Vector2(-130f, -150f), new Vector2(220f, 70f), new Color(0.1f, 0.6f, 0.2f), OnExport);
        MakeButton("Close", new Vector2(130f, -150f), new Vector2(220f, 70f), new Color(0.7f, 0.2f, 0.2f), () => root.SetActive(false));

        statusText = MakeText("Status", "", 28, new Vector2(760f, 80f), new Vector2(0f, -235f));
        root.SetActive(false);
    }

    void MakeImage(string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(root.transform, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = color; img.raycastTarget = false;
    }

    TMP_Text MakeText(string name, string text, float size, Vector2 rectSize, Vector2 pos, Transform parent = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent != null ? parent : root.transform, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = rectSize; rt.anchoredPosition = pos;
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    TMP_Text MakeButton(string label, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn " + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(root.transform, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        go.GetComponent<Image>().color = color;
        go.GetComponent<Button>().onClick.AddListener(onClick);
        return MakeText("Label", label, size.y * 0.55f, size, Vector2.zero, go.transform);
    }
}
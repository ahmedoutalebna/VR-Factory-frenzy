using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ResultsScreen : MonoBehaviour
{
    public static ResultsScreen Instance { get; private set; }
    public static int DeathCount;

    GameObject canvasRoot;
    TMP_Text label;

    void Awake()
    {
        Instance = this;
        DeathCount = 0;
        BuildUI();
    }

    void BuildUI()
    {
        canvasRoot = new GameObject("ResultsCanvas");
        canvasRoot.transform.SetParent(transform, false);

        var canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // dark background
        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasRoot.transform, false);
        Stretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        // text
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(canvasRoot.transform, false);
        Stretch(textObj.GetComponent<RectTransform>());
        label = textObj.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 80;
        label.color = Color.white;

        canvasRoot.SetActive(false);      // hidden until the race ends
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // RaceManager still calls Show(time), so the parameter stays but is not displayed
    public void Show(float time)
    {
        int died = Mathf.Max(0, DeathCount - 1);   // the first Respawn() is the start placement, not a death

        string deaths = died == 0 ? "You didn't die once!"
                      : died == 1 ? "You only died 1 time"
                      : $"You only died {died} times";

        label.text = $"Congratulations!\n\n{deaths}\n\n<size=40>Press R to restart</size>";
        canvasRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // optional extra (not in the spec): restart with R after finishing
        if (canvasRoot != null && canvasRoot.activeSelf && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
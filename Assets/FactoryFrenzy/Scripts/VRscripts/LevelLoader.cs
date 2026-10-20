using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [System.Serializable]
    public class Entry { public string typeId; public GameObject prefab; }

    [Header("typeId -> PC prefab (the real game prefabs, not the editor variants)")]
    public Entry[] registry;

    [Header("Test without the menu: type a file name here (without .json)")]
    public string levelToLoad;

    public static string SelectedLevel;   // will be set by the main menu (L1)
    public static string Folder => Path.Combine(Application.persistentDataPath, "Levels");

    // Awake, so everything exists before the other scripts run their Start()
    void Awake()
    {
        string levelName = !string.IsNullOrEmpty(SelectedLevel) ? SelectedLevel : levelToLoad;
        if (string.IsNullOrEmpty(levelName)) return;   // nothing chosen: the scene stays as it is
        Load(levelName);
    }

    void Load(string levelName)
    {
        string path = Path.Combine(Folder, levelName + ".json");
        if (!File.Exists(path)) { Debug.LogError("Level not found: " + path); return; }

        LevelData data = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));

        // positions in the JSON are relative to the "Map" object
        var mapObj = GameObject.Find("Map");
        Transform anchor = mapObj != null ? mapObj.transform : null;
        if (anchor == null) Debug.LogWarning("No 'Map' object found: using world positions.");

        Vector3 ToWorld(Vector3 local) => anchor != null ? anchor.TransformPoint(local) : local;
        Quaternion ToWorldRot(Vector3 euler) =>
            (anchor != null ? anchor.rotation : Quaternion.identity) * Quaternion.Euler(euler);

        var root = new GameObject("Level_" + levelName).transform;

        foreach (var p in data.pieces)
        {
            GameObject prefab = null;
            foreach (var e in registry) if (e.typeId == p.typeId) { prefab = e.prefab; break; }
            if (prefab == null) { Debug.LogWarning("No prefab for typeId: " + p.typeId); continue; }

            var go = Instantiate(prefab, ToWorld(p.position), ToWorldRot(p.rotation), root);

            if (p.isMoving)
            {
                var mp = go.GetComponent<MovingPlatform>();
                if (mp != null)
                {
                    mp.rotationAxis = p.moving.rotationAxis;
                    mp.rotationSpeed = p.moving.rotationSpeed;
                    mp.Configure(ToWorld(p.moving.start), ToWorld(p.moving.end), p.moving.travelTime);
                }
            }
        }
        Debug.Log("Level loaded: " + levelName + " (" + data.pieces.Count + " pieces)");
    }
}
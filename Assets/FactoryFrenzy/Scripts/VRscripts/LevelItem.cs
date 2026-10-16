using System.Collections.Generic;
using UnityEngine;

public class LevelItem : MonoBehaviour
{
    public static readonly HashSet<LevelItem> HeldItems = new HashSet<LevelItem>();
    public static bool DeleteMode;                 // toggled by the UI button (option D)

    public string typeId = "tree";                 // used later by the JSON export (FP1-5)
    public Color deleteHighlight = new Color(1f, 0.2f, 0.2f);

    Renderer[] renderers;
    MaterialPropertyBlock block;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
    }

    // Select Entered
    public void OnGrabbed()
    {
        if (DeleteMode) { Destroy(gameObject); return; }   // option D: grabbing = deleting
        HeldItems.Add(this);
    }

    // Select Exited
    public void OnReleased() { HeldItems.Remove(this); }

    // Hover Entered / Hover Exited
    public void OnHoverEntered() { if (DeleteMode) SetTint(true); }
    public void OnHoverExited() { SetTint(false); }

    void SetTint(bool on)
    {
        foreach (var r in renderers)
        {
            if (on) { block.SetColor("_BaseColor", deleteHighlight); r.SetPropertyBlock(block); }
            else r.SetPropertyBlock(null);
        }
    }

    void OnDestroy() { HeldItems.Remove(this); }
}
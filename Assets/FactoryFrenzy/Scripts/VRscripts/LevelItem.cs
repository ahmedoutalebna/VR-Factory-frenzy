using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelItem : MonoBehaviour
{
    public static readonly HashSet<LevelItem> HeldItems = new HashSet<LevelItem>();   // unlocked items in hand
    public static readonly HashSet<LevelItem> Targeted = new HashSet<LevelItem>();    // hovered or grabbed
    public static bool DeleteMode;
    public static LevelItem Current;   // the single item the lock button acts on

    public string typeId = "tree";
    public bool locked;
    public Color deleteHighlight = new Color(1f, 0.2f, 0.2f);
    public Color lockedColor = new Color(0.3f, 0.6f, 1f);

    Renderer[] renderers;
    MaterialPropertyBlock block;
    Component grab;
    TextMeshPro label;
    bool hovered, selected;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        grab = GetComponent("XRGrabInteractable");   // looked up by name so it works on XRI 2.x and 3.x
        BuildLabel();
        SetLocked(locked);
    }

    void BuildLabel()
    {
        Bounds b = renderers.Length > 0 ? renderers[0].bounds : new Bounds(transform.position, Vector3.one);
        foreach (var r in renderers) b.Encapsulate(r.bounds);

        var go = new GameObject("LockLabel");
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(b.center.x, b.max.y + 0.2f, b.center.z);
        go.transform.localScale = Vector3.one / Mathf.Max(0.0001f, transform.lossyScale.x);

        label = go.AddComponent<TextMeshPro>();
        label.text = "LOCKED";
        label.fontSize = 3f;                          // raise or lower if too big or small
        label.alignment = TextAlignmentOptions.Center;
        label.color = lockedColor;
    }

    // ---- events wired in the Inspector ----
    public void OnGrabbed()                           // Select Entered
    {
        selected = true;
        Current = this;
        UpdateTargeted();
        if (locked) return;                           // a locked item is never held or deleted
        if (DeleteMode) { DeleteSound.Play(transform.position); Destroy(gameObject); return; }
        HeldItems.Add(this);
    }

    public void OnReleased()                          // Select Exited
    {
        selected = false;
        HeldItems.Remove(this);
        UpdateTargeted();
    }

    public void OnHoverEntered() { hovered = true; Current = this; UpdateTargeted(); RefreshVisual(); }

    public void OnHoverExited()
    {
        hovered = false;
        if (Current == this && !selected) Current = null;   // only clear if nobody newer took over
        UpdateTargeted(); RefreshVisual();
    }

    // ---- lock ----
    public void ToggleLock() { SetLocked(!locked); }

    public void SetLocked(bool value)
    {
        locked = value;
        if (locked) HeldItems.Remove(this);
        else if (selected) HeldItems.Add(this);

        // a locked item keeps its place even if it is grabbed
        if (grab != null)
        {
            var t = grab.GetType();
            t.GetProperty("trackPosition")?.SetValue(grab, !locked);
            t.GetProperty("trackRotation")?.SetValue(grab, !locked);
        }
        RefreshVisual();
    }

    void UpdateTargeted()
    {
        if (hovered || selected) Targeted.Add(this); else Targeted.Remove(this);
    }

    void RefreshVisual()
    {
        bool tint = false; Color c = Color.white;
        if (DeleteMode && hovered && !locked) { tint = true; c = deleteHighlight; }
        else if (locked) { tint = true; c = lockedColor; }

        foreach (var r in renderers)
        {
            if (tint) { block.SetColor("_BaseColor", c); r.SetPropertyBlock(block); }
            else r.SetPropertyBlock(null);
        }
        if (label != null) label.gameObject.SetActive(locked);
    }

    void LateUpdate()   // the label always faces the player
    {
        if (!locked || label == null || Camera.main == null) return;
        label.transform.rotation = Quaternion.LookRotation(label.transform.position - Camera.main.transform.position);
    }

    void OnDestroy()
    {
        HeldItems.Remove(this); Targeted.Remove(this);
        if (Current == this) Current = null;
    }
}
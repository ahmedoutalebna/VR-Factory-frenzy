using UnityEngine;

public static class EditorSounds
{
    static AudioClip spawn, snap;

    public static void PlaySpawn(Vector3 pos)
    {
        if (spawn == null) spawn = Tone(800f, 0.08f);       // short, light pop
        AudioSource.PlayClipAtPoint(spawn, pos, 0.6f);
    }

    public static void PlaySnap(Vector3 pos)
    {
        if (snap == null) snap = Tone(600f, 0.15f, 1.5f);   // slightly longer, rising "click into place"
        AudioSource.PlayClipAtPoint(snap, pos, 0.7f);
    }

    // glide > 1 makes the pitch rise during the sound
    static AudioClip Tone(float freq, float len, float glide = 1f)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * len);
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = Mathf.Lerp(freq, freq * glide, t / len);
            phase += 2f * Mathf.PI * f / rate;
            data[i] = Mathf.Sin(phase) * Mathf.Exp(-t * 5f / len) * 0.5f;
        }
        var c = AudioClip.Create("fx", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
using UnityEngine;

public static class DeleteSound
{
    static AudioClip clip;

    // Optional: assign your own clip here from another script. Otherwise a short low beep is generated.
    public static AudioClip customClip;

    public static void Play(Vector3 position, float volume = 0.7f)
    {
        var c = customClip != null ? customClip : (clip != null ? clip : (clip = MakeBeep()));
        // PlayClipAtPoint keeps playing even though the item is destroyed right after
        AudioSource.PlayClipAtPoint(c, position, volume);
    }

    static AudioClip MakeBeep()
    {
        int rate = 44100;
        float len = 0.18f, freq = 250f;
        int n = Mathf.CeilToInt(rate * len);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 5f / len) * 0.5f;
        }
        var c = AudioClip.Create("delete", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
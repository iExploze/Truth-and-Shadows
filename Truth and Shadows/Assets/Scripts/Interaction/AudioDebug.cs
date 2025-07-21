using System.Collections.Generic;
using UnityEngine;

public class AudioDebug : MonoBehaviour
{
    public float checkInterval = 2f;
    private float timer;

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            LogAllPlayingAudio();
        }
    }

    void LogAllPlayingAudio()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        Debug.Log("=== Playing AudioSources ===");

        int count = 0;
        foreach (var source in sources)
        {
            if (source.isPlaying && source.clip != null)
            {
                count++;
                Debug.Log($"🎵 '{source.clip.name}' on '{source.gameObject.name}'");
            }
        }

        if (count == 0)
        {
            Debug.Log("No audio currently playing.");
        }
    }
}

using UnityEngine;

public class audiotest : MonoBehaviour
{
    [Range(0f, 1f)]
    public float volume = 1f; // Set this in Inspector

    void Start()
    {
        AudioListener.volume = volume;
    }

    private void Update()
    {
        Debug.Log(AudioListener.volume);
        AudioListener.volume = volume;
    }
}

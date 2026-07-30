using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxAudioSource;
    public AudioClip runningAudioClip;
    void Start()
    {
        sfxAudioSource.clip = runningAudioClip;
        sfxAudioSource.loop = true;
        sfxAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

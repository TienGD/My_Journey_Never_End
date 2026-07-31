using UnityEngine;
public class LV1Audio : MonoBehaviour
{
    public AudioSource sfxAudioSource;
    public AudioSource bgmAudioSource;
    public AudioClip runningAudioClip;
    public AudioClip backgroundMusicClip;
    void Start()
    {
        sfxAudioSource.clip = runningAudioClip;
        sfxAudioSource.loop = true;
        sfxAudioSource.Play();

        bgmAudioSource.clip = backgroundMusicClip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

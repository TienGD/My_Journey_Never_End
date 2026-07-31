using UnityEngine;

public class LV1Audio : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip musicClip;
    public AudioClip runningClip;
    void Start()
    {
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
        sfxSource.clip = runningClip;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

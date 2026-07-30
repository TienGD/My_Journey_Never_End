using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Running : MonoBehaviour
{
    private Player player;
    private AudioSource runningAudioSource;

    [SerializeField] private float minSpeed = 0.1f;

    private void Awake()
    {
        player = GetComponent<Player>();
        runningAudioSource = GetComponent<AudioManager>().sfxAudioSource;

        runningAudioSource.playOnAwake = true;
        runningAudioSource.loop = true;
    }

    private void Update()
    {
        if (player == null)
            return;

        bool isRunning = player.velocity.magnitude > minSpeed;

        if (isRunning)
        {
            if (!runningAudioSource.isPlaying)
                runningAudioSource.Play();
        }
        else
        {
            if (runningAudioSource.isPlaying)
                runningAudioSource.Stop();
        }
    }
}
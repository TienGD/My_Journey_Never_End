using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class Scene : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] float time=0;

    private void Update()
    {
        time += Time.deltaTime;
        float currentTime= Mathf.FloorToInt(time * 10);
        Debug.Log($"time:{currentTime}");

        if (currentTime == 1500)
        {
            enoughDistance();
        }
    }

    public void enoughDistance()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            player.isChangingScene = true;
        }
        StartCoroutine(loadAsyncSence(sceneName));
    }

    IEnumerator loadAsyncSence(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // While the asynchronous operation to load the new
        // scene is not yet complete, continue waiting until
        // it's done.
        while (!asyncLoad.isDone)
        {
            // Here, you can also update the loading screen
            // progress bar or any loading indicators you have.
            yield return null;
        }
    }
}

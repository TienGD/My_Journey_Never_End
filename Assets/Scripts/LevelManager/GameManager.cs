using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int coins;

    private void Awake()
    {
        instance = this;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }
}

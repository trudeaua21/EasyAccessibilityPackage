using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public void loadMainMenuScene()
    {
        SceneManager.LoadScene("SRMainMenu", LoadSceneMode.Single);
    }

    public void loadGameScene()
    {
        SceneManager.LoadScene("SRGame", LoadSceneMode.Single);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}

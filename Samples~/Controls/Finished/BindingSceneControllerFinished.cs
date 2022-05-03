using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple scene navigation tool. This is not meant to demonstrate 
/// any best practices in Unity Scene Management - this was just built for 
/// use within this demo.
/// </summary>
public class BindingSceneControllerFinished : MonoBehaviour
{
   
    public void loadSettings()
    {
        SceneManager.LoadScene("EAP-RebindingUISample-FINISHED");
    }

    public void loadGame()
    {
        SceneManager.LoadScene("RebindGame-FINISHED");
    }

    public void exitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Example player controller class that outputs the current actions being taken.
/// Any logic you see here has nothing to do with Input System best practices - this is 
/// for demonstration purposes only.
/// </summary>
public class BindingPlayerControllerFinished : MonoBehaviour
{

    public Text outputText;

    private bool interactFlag = false;
    private Vector2 moveVector = new Vector2(0.0f, 0.0f);
    private Vector2 lookVector = new Vector2(0.0f, 0.0f);

    // update the output text with the current input states
    void Update()
    {
        string outputString = "";

        if (outputText)
        {
            if (interactFlag)
            {
                outputString += "Interacting...";
            }

            if (!moveVector.Equals(Vector2.zero))
            {
                outputString += "\nMove value is " + moveVector.ToString();
            }

            if (!lookVector.Equals(Vector2.zero))
            {
                outputString += "\nLook value is " + lookVector.ToString();
            }

            // if the output string is still blank, no inputs detected
            if (outputString.Length == 0)
            {
                outputString = "No action.";
            }

            outputText.text = outputString;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) {
            interactFlag = true;  
        }
        else if (context.canceled)
        {
            interactFlag = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookVector = context.ReadValue<Vector2>();
    }

    private void TryOutputAction(string output)
    {
        if (outputText)
            outputText.text = output;
        else
            Debug.Log(output);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DavyKager;

/// <summary>
/// A component that provides Screen Reader functionality using the <see cref="Tolk"/> library.
/// 
/// This component has two main ways to output text to a screen reader:
/// - Provides a public static method (StaticReadText) allowing the reading of 
///   text via script
/// - Provides public member methods callable via Unity Event that can read from a string,
///   from a Text component, or from a ScreenReaderOutput component.
/// 
/// Additionally, this component contains logic to track a list of focusable 
/// <see cref="GameObject"/>'s to highlight and read a message for. This list can be navigated via 
/// keyboard input by default.
/// </summary>
/// 
/// <remarks>
/// Each focusable <see cref="GameObject"/> must have a <see cref="Outline"/> component attached to
/// it in order for it to be focused on. This outline will be enabled if the item receives focus,
/// and disabled if the item is not focused.
/// 
/// Additionally, each focusable <see cref="GameObject"/> must have a <see cref="ScreenReaderOutput"/>
/// component attached to it in order to be focused on. This is becuase the focus system gets the 
/// screen reader output for the <see cref="GameObject"/> from the <see cref="ScreenReaderOutput"/>
/// component.
/// 
/// Nothing will recieve focus until the key for moving focus forward or backward is pressed.
/// 
/// The default controls for the focus system are as follows:
/// - Move focus forward - Tab
/// - Move focus backward - Hold Shift + Press Tab
/// - Interact with focused item - Return
/// - Re-read focused item - Right Control
/// 
/// To modify these, simply change the keys within this class's update function, or better yet, use the new Unity Input
/// System and map them to actions rather than keys! 
/// To do this, see https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/QuickStartGuide.html
/// 
/// The Tolk library is provided by Davy Kager, and used under the GNU Lesser 
/// General Public License (Version 3, 29 June 2007). Makefiles within the project have been
/// modified to excluded the building of unused libraries (such as the Java wrapper), but the 
/// underlying library code from the DLLs has not been modified.
/// 
/// Find the Tolk library here: https://github.com/dkager/tolk
/// </remarks>
public class ScreenReader : MonoBehaviour
{

    /// <summary>
    /// It is good pracitice to display a message when the scene first loads detailing
    /// the contents/purpose of the current screen. This variable is read on scene
    /// load to fulfill that purpose.
    /// </summary>
    public string sceneEnterMessage;

    /// <summary>
    /// A list of UI objects for which the screen reader will read messages
    /// </summary>
    public List<GameObject> readableUIObjects;

    /// <summary>
    /// The index of the currently focused <see cref="GameObject"/>
    /// </summary>
    private int currentFocusedIndex;

    /// <summary>
    /// The Unity event system that's controlling the current UI scene.
    /// </summary>
    private EventSystem currentEventSystem;

    /// <summary>
    /// A message detailing controls for the screen reader.
    /// </summary>
    private string controlsMessage;

    /// <summary>
    /// Loads the <see cref="Tolk"/> library.
    /// </summary>
    void Awake()
    {
        controlsMessage = "Press Tab key to focus on UI objects, Return to interact with buttons, and Right"
            + "Control key to re-read the focused UI object";

        Debug.Log("Loading Tolk...");
        Tolk.Load();
        Debug.Log("Querying for the active screen reader driver...");
        string name = Tolk.DetectScreenReader();
        if (name != null)
        {
            Debug.Log("The active screen reader driver is: " + name);
        }
        else
        {
            Debug.Log("None of the supported screen readers is running");
        }
        Debug.Log("Tolk has finished");
    }

    /// <summary>
    /// The focused index will start at -1 - nothing will be focused.
    /// 
    /// When we start, we will check all the <see cref="GameObject"/>s in our list
    /// to ensure that they 1 - have an associated <see cref="Outline"/> component
    /// and 2 - have an associated <see cref="ScreenReaderOutput"/> component detailing
    /// what should be output by the screen reader for them. If these components
    /// are not present, error and quit the application.
    /// </summary>
    void Start()
    {
        currentEventSystem = EventSystem.current;

        foreach(GameObject obj in readableUIObjects)
        {
            if(!obj.TryGetComponent<Outline>(out Outline outline) && !obj.TryGetComponent<ScreenReaderOutput>(out ScreenReaderOutput output))
            {
                Debug.LogError("Object \"" + obj.name + "\" from the Screen Reader's focus list does not have the required components." +
                    "Please ensure each object in this list has an Outline component and a ScreenReaderOutput component attached, and make sure" +
                    "that the ScreenReaderOutput component is in a valid state for its GameObject.");
                Application.Quit();
            }
        }

        currentFocusedIndex = -1;

        StaticReadText(sceneEnterMessage);

        StaticReadText(controlsMessage);
    }

    /// <summary>
    /// Check for input and to change the currently focused <see cref="GameObject"/> and read a message for the newly
    /// focused <see cref="GameObject"/>.
    /// </summary>
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&  Input.GetKeyDown(KeyCode.Tab))
        {
            MoveFocusBackward();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            MoveFocusForward();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            PerformActionOnFocusedObject();
        }

        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            ReadFocusedObjectOutput();
        }
    }

    /// <summary>
    /// When this object is destoryed (when the scene changes), unload
    /// the <see cref="Tolk"/> library.
    /// </summary>
    private void OnDestroy()
    {
        Debug.Log("Unloading Tolk for scene change...");
        Tolk.Unload();
    }

    /// <summary>
    /// On application quit, unload the <see cref="Tolk"/> library.
    /// </summary>
    void OnApplicationQuit()
    {
        Debug.Log("Unloading Tolk...");
        StaticReadText("Exiting game...");
        Tolk.Unload();
    }

    /// <summary>
    /// Cycles focus to the next <see cref="GameObject"/> (currently selected index in the list + 1)
    /// and reads its message.
    /// 
    /// Will error and quit the applicaiton if the <see cref="GameObject"/> has no
    /// <see cref="Outline"/> component attached.
    /// </summary>
    private void MoveFocusForward()
    {
        if(readableUIObjects.Count == 0)
        {
            StaticReadText("No focusable objects in scene.");
            return;
        }

        // disable the outline on the currently selected compenent (if there is one currently selected)
        if (currentFocusedIndex > -1)
        {
            SetOutlineEnabled(readableUIObjects[currentFocusedIndex], false);
        }
        
        // If we reach the end of the list, focus on the first list entry
        currentFocusedIndex = (currentFocusedIndex + 1) % readableUIObjects.Count;

        // enable the outline of the new selection and read its message
        SetOutlineEnabled(readableUIObjects[currentFocusedIndex], true);

        // Set the new selection as the current selected game object in the event system
        currentEventSystem.SetSelectedGameObject(readableUIObjects[currentFocusedIndex]);

        // read the output of the new selection
        ReadFocusedObjectOutput();
        
    }

    /// <summary>
    /// Cycles focus to the previous <see cref="GameObject"/> (currently selected index in the list - 1)
    /// and reads its message.
    /// 
    /// Will error and quit the applicaiton if the <see cref="GameObject"/> has no
    /// <see cref="Outline"/> component attached.
    /// </summary>
    private void MoveFocusBackward()
    {
        if (readableUIObjects.Count == 0)
        {
            StaticReadText("No focusable objects in scene.");
            return;
        }

        // disable the outline on the currently selected compenent (if there is one currently selected)
        if (currentFocusedIndex > -1)
        {
            SetOutlineEnabled(readableUIObjects[currentFocusedIndex], false);
        }

        // Focus on the previous index
        currentFocusedIndex--;

        // If we reach the beginning of the list, loop back to the end
        if (currentFocusedIndex < 0)
            currentFocusedIndex = readableUIObjects.Count - 1;

        // enable the outline of the new selection and read its message
        SetOutlineEnabled(readableUIObjects[currentFocusedIndex], true);

        // Set the new selection as the current selected game object in the event system
        currentEventSystem.SetSelectedGameObject(readableUIObjects[currentFocusedIndex]);

        // read the output of the new selection
        ReadFocusedObjectOutput();
    }

    /// <summary>
    /// Performs a contextual action on the focused object. Currently, this 
    /// method serves no purpose, but if a need arises for a custom contextual
    /// action on the focused item, this would be where to put it.
    /// </summary>
    private void PerformActionOnFocusedObject()
    {
        GameObject gameObject = readableUIObjects[currentFocusedIndex];

        // perform a contextual action on the game object
    }

    /// <summary>
    /// Sets the "enabled" property of the <see cref="Outline"/> component on 
    /// the given <see cref="GameObject"/> to the given boolean.
    /// 
    /// If the given <see cref="GameObject"/> has no <see cref="Outline"/>
    /// component, logs an error and quits the application.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to update.</param>
    /// <param name="enabled">The boolean to set the "enabled" state to.</param>
    private void SetOutlineEnabled(GameObject gameObject, bool enabled)
    {
        if (gameObject.TryGetComponent<Outline>(out Outline outline))
            outline.enabled = enabled;
        else
        {
            Debug.LogError("Focusable gameobject \"" + gameObject.name + "\" has no outline component.");
            Application.Quit();
        }
    }

    /// <summary>
    /// Reads the output of the currently focused <see cref="GameObject"/>. What to read
    /// is determined by the content of the <see cref="ScreenReaderOutput"/> component that
    /// is attached to the <see cref="GameObject"/>. If there is no attached <see cref="ScreenReaderOutput"/>
    /// component, then an error is logged and the application exits.
    /// </summary>
    private void ReadFocusedObjectOutput()
    {
        if (currentFocusedIndex == -1)
        {
            StaticReadText("No item is focused. Press tab to focus on an item.");
            return;
        }

        GameObject gameObject = readableUIObjects[currentFocusedIndex];

        if (gameObject.TryGetComponent<ScreenReaderOutput>(out ScreenReaderOutput output))
        {
            ReadFromOutputObject(output);
        }
        else
        {
            Debug.LogError("Focusable gameobject \"" + gameObject.name + "\" has no ScreenReaderOutput component.");
            Application.Quit();
        }
    }

    /// <summary>
    /// Calls <see cref="StaticReadText(string)"/> for the given string.
    /// Included to allow this Component to be used in UnityEvents.
    /// </summary>
    /// <param name="text">The text for the screen reader to read.</param>
    public void ReadText(string text)
    {
        StaticReadText(text);
    }

    /// <summary>
    /// Calls <see cref="StaticReadText(string)"/> on the "text" property
    /// of the given <see cref="Text"/> component.
    /// </summary>
    /// <param name="textObject">The <see cref="Text"/> component to read from.</param>
    public void ReadFromTextObject(Text textObject)
    {
        StaticReadText(textObject.text);
    }

    /// <summary>
    /// Calls <see cref="StaticReadText(string)"/> on the contents of the 
    /// given <see cref="ScreenReaderOutput"/> component.
    /// </summary>
    /// <param name="outputObject">The <see cref="ScreenReaderOutput"/> component to read from.</param>
    public void ReadFromOutputObject(ScreenReaderOutput outputObject)
    {
        StaticReadText(outputObject.getScreenReaderOutput());
    }


    ///////////////////////////////////////////////////////////////////
    /// Static methods (to access from scripts)
    ///////////////////////////////////////////////////////////////////

    /// <summary>
    /// Has the current screen reader read the given string via the 
    /// <see cref="Tolk"/> library. 
    /// </summary>
    public static void StaticReadText(string outputText)
    {
        Debug.Log("Trying to read " + outputText);
        if (!Tolk.Output(outputText))
        {
            Debug.Log("Failed to output text");
        }
    }
}

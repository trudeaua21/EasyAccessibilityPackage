using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// A component wrapper around a single string that will contain
/// the text that the <see cref="GameObject"/> this is attached to
/// should pass to the screen reader. It can read either a passed 
/// string, or text from a <see cref="Text"/> component attached 
/// to the <see cref="GameObject"/> this is attached to.
/// </summary>
/// 
/// <remarks>
/// This script automatically uses the <see cref="IPointerEnterHandler"/> to detect
/// pointer enter events so that the message for this <see cref="GameObject"/> is 
/// read on hover. If you do not want this functionality, ensure that <see cref="readOnHover"/>
/// is false.
/// 
/// This script currently hard-codes in a <see cref="Outline"/> object on the GameObject if none is present.
/// There is no way to override this, but the outline will be disabled by default. The outline that 
/// is currently on the GameObject will be used instead if it is present.
/// 
/// The text given to this component is hard-coded to be read when the <see cref="GameObject"/> receives focus 
/// from this Scene's ScreenReader component.
/// 
/// The component will log an error if (1) it attempts to read text from an attached 
/// <see cref="Text"/> component when there is none attached or (2) if the string 
/// passed to this component is whitespace or null. In either of these cases, the 
/// screen reader output for this component will be "This object has no associated 
/// message".
/// </remarks>
public class ScreenReaderOutput : MonoBehaviour, IPointerEnterHandler
{
    // The public fields to show in the editor
    public bool readFromTextObject;
    public bool readOnHover;
    public string outputText;

    // The string the ScreenReader class will read
    private string screenReaderPassedText;

    private Text textComponent;

    /// <summary>
    /// Add the <see cref="Outline"/> component in Awake so that
    /// it's there when the <see cref="ScreenReader"/> object looks for 
    /// it in Start to set up the tab indexing
    /// </summary>
    void Awake()
    {
        // will only add the outline at runtime if one doesn't already exist
        if(!this.gameObject.TryGetComponent<Outline>(out Outline testOutline))
        {
            Outline outline = this.gameObject.AddComponent<Outline>() as Outline;

            // the outline will have different effectDistance on a text object vs any other
            if (readFromTextObject)
                outline.effectDistance = new Vector2(3f, -1.5f);
            else
                outline.effectDistance = new Vector2(6.76f, -7.67f);

            outline.effectColor = new Color(0.04705882f, 0, 0.8627451f, 1);
            outline.useGraphicAlpha = true;

            outline.enabled = false;
        }
    }

    void Start()
    {
        bool textComponentPresent = this.gameObject.TryGetComponent<Text>(out Text textObject);

        // If we're reading from a Text object
        if (readFromTextObject) {
            // Attempt to find the attached Text component
            if(textComponentPresent)
            {
                textComponent = textObject;
                screenReaderPassedText = textObject.text;
            }
            else
            {
                Debug.LogError("There is no valid Text component attached to the GameObject \"" + this.gameObject.name + "\".");
                screenReaderPassedText = "This object has no associated message";
            }
        }
        else
        {
            // we cannot read whitespace or a null string
            if (string.IsNullOrWhiteSpace(outputText))
            {
                screenReaderPassedText = "This object has no associated message";
            }
            else
            {
                screenReaderPassedText = outputText;
            }
        }
    }

    /// <summary>
    /// If we're reading from a Text object, track and save any changes to that object
    /// </summary>
    void Update()
    {
        if (readFromTextObject)
        {
            if (!textComponent.text.Equals(screenReaderPassedText))
            {
                screenReaderPassedText = textComponent.text;
            }
        }
    }

    /// <summary>
    /// Read the string from the 
    /// </summary>
    /// <param name="pointerEventData">Data from the OnPointerEnter event.</param>
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if(readOnHover)
            ScreenReader.StaticReadText(screenReaderPassedText);
    }

    /// <summary>
    /// Gets the final string to be output to the ScreenReader.
    /// </summary>
    /// <returns>The message to read in the ScreenReader.</returns>
    public string getScreenReaderOutput()
    {
        return screenReaderPassedText;
    }
}

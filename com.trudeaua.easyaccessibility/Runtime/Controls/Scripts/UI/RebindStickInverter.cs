using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.Samples.RebindUI;

// TODO Make it so that this isn't hard-coded to be a child of RebindUI, and can just have stuff passed in


public class RebindStickInverter : MonoBehaviour
{


    public Toggle invertXToggle;
    public Toggle invertYToggle;


    private bool invertX;
    private bool invertY;

    private const string processorName = "InvertVector2";

    private InputAction actionToBind;
    private int bindingIndex;

    void Start()
    {
        RebindActionUI rebindUIComponent = gameObject.GetComponentInParent<RebindActionUI>();
        if(rebindUIComponent == null)
        {
            Debug.LogError("Cannot find RebindActionUI component on parent object - is this attached to a RebindUIPrefab?");
        }

        actionToBind = rebindUIComponent.actionReference.action;

        if(actionToBind == null)
        {
            Debug.LogError("No valid action associated with the parent RebindActionUI component");
        }

        // Look up binding index (code taken from RebindActionUI)
        var bindingId = new Guid(rebindUIComponent.bindingId);
        bindingIndex = actionToBind.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{actionToBind}'", this);
        }

        // if we have a saved value for the processor, load it. otherwise, default to neither axis inverted
        if (BindingSaveLoader.BindingHasSavedProcessors(actionToBind, bindingIndex))
        {
            if(BindingSaveLoader.ProcessorIsSavedForBinding(actionToBind, bindingIndex, "InvertVector2"))
            {
                Debug.Log("InvertVector2 processor value has been found");
                LoadProcessor();
            }
            else
            {
                invertX = false;
                invertY = false;
                updateToggles();
                SaveProcessor();
            }
        }
        else
        {
            invertX = false;
            invertY = false;
            updateToggles();
            SaveProcessor();
        }
    }


    public void onXToggleChange(bool state)
    {
        invertX = state;
        SaveProcessor();
    }

    public void onYToggleChange(bool state)
    {
        invertY = state;
        SaveProcessor();
    }

    private void updateToggles()
    {
        this.invertXToggle.isOn = invertX;
        this.invertYToggle.isOn = invertY;
    }

    

    /// <summary>
    /// Updates the stored value for the InvertVector2Proecessor.
    /// </summary>
    private void SaveProcessor()
    {
        BindingSaveLoader.SaveProcessor(actionToBind, bindingIndex, "InvertVector2", getCurrentArgumentsString(), false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    private void LoadProcessor()
    {
        if (!BindingSaveLoader.BindingHasSavedProcessors(actionToBind, bindingIndex))
        {
            throw new InvalidOperationException("There are no processors stored for action " + actionToBind.name + "and binding " + actionToBind.bindings[bindingIndex]);
        }

        // get the processors/arguments from prefs
        string processors = BindingSaveLoader.GetSavedProcessors(actionToBind, bindingIndex);

        // if there is no invertVector2 processor to load from, that's an error
        if (!ProcessorStringHelper.hasProcessor(processors, "InvertVector2"))
        {
            throw new InvalidOperationException("There is no InvertVector2 processor stored for action " + actionToBind.name + "and binding " + actionToBind.bindings[bindingIndex]);
        }

        string argumentsToLoad = ProcessorStringHelper.GetProcessorArguments(processors, "InvertVector2");

        // Split the string up based on the comma in the argument list
        string[] splitString = argumentsToLoad.Split(',');

        if (splitString.Length != 2)
        {
            throw new ArgumentException("Invalid key - key should be formatted like \"invertX=[bool], invertY=[bool]\" - was \"" + argumentsToLoad + "\"");
        }

        // The values we're looking for are the string after the '=' in each substring - store them in local state
        this.invertX = bool.Parse(splitString[0].Substring(splitString[0].IndexOf('=') + 1));
        this.invertY = bool.Parse(splitString[1].Substring(splitString[1].IndexOf('=') + 1));

        // update the toggles to reflect the current state of the processors applied
        updateToggles();
    }

    /// <summary>
    /// Generates the string for applying the Invert Vector2 processor.
    /// </summary>
    /// <returns>The full string needed to apply this processor to a binding</returns>
    private string getCurrentProcessorString()
    {
        return "InvertVector2(" + getCurrentArgumentsString() +  ")";
    }

    /// <summary>
    /// Generates the argumets string to apply to the Invert Vector2 processor, taking
    /// the x and y values from local state.
    /// </summary>
    /// <returns>The arguments string for the Invert Vector2 processor</returns>
    private string getCurrentArgumentsString()
    {
        return "invertX = " + invertX.ToString() + ", invertY = " + invertY.ToString();
    }
}

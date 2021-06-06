using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;


// TODO Make it so that this isn't hard-coded to be a child of RebindUI, and can just have stuff passed in
// TODO Make an IProcessorUI interface to force implement of save/load processor and getCurrentArgumentsString


public class RebindStickNorth : MonoBehaviour
{

    public Dropdown dropdownList;

    // the number of 45 degree angles to apply to the right to set stick north
    private int num45ToRight;

    private const string processorName = "ChangeStickNorth";

    // the action/binding we're applying the processor to
    private InputAction actionToBind;
    private int bindingIndex;

    void Start()
    {
        // will start here by default, may be overriden by load
        num45ToRight = 0;

        RebindActionUI rebindUIComponent = gameObject.GetComponentInParent<RebindActionUI>();
        if (rebindUIComponent == null)
        {
            Debug.LogError("Cannot find RebindActionUI component on parent object - is this attached to a RebindUIPrefab?");
        }

        actionToBind = rebindUIComponent.actionReference.action;

        if (actionToBind == null)
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

        LoadProcessor();
    }


    // save the processor so it persists between play sessions
    private void SaveProcessor()
    {
        BindingSaveLoader.SaveProcessor(actionToBind, bindingIndex, processorName, getCurrentArgumentsString(), true);
    }

    
    private void LoadProcessor()
    {
        // if this processor isn't saved for the current action/binding pair, we're done here
        if(!BindingSaveLoader.ProcessorIsSavedForBinding(actionToBind, bindingIndex, processorName))
        {
            return;
        }

        // otherwise, find the current saved value and apply it to our dropdown
        string processors = BindingSaveLoader.GetSavedProcessors(actionToBind, bindingIndex);

        string argumentsToLoad = ProcessorStringHelper.GetProcessorArguments(processors, processorName);

        string parameterFromArguments = argumentsToLoad.Substring(argumentsToLoad.IndexOf('=') + 1);

        // parse the int from the part of the arguments after the equals sign
        num45ToRight = int.Parse(parameterFromArguments);

        // apply the loaded value to the dropdown list
        updateDropdown();
    }

    private void updateDropdown()
    {
        dropdownList.value = num45ToRight;
    }

    public void onDropdownChange(int selectedIndex)
    {
        num45ToRight = selectedIndex;
        SaveProcessor();
    }

    private string getCurrentArgumentsString()
    {
        return "num45ToRight=" + num45ToRight;
    }

}

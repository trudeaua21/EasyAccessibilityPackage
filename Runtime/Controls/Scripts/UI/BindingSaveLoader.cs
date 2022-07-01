using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Provides methods to save and load given Action/Binding pairings to PlayerPrefs,
/// which allows bindings to persist between scenes and play sessions.
/// 
/// The key in PlayerPrefs that a specific action/binding pair will correspond to
/// is the string representation of the action's id, followed by the binding index.
/// 
/// <remarks>
/// NOTE: If you change parts of your control mappings outside of this class,
/// this class may not function as intended.
/// </remarks>
/// </summary>
public class BindingSaveLoader : MonoBehaviour
{
    public InputActionAsset actionAsset;

    public bool shouldLoad;

    /// <summary>
    /// Load bindings when the component first loads
    /// </summary>
    void Awake()
    {
        //PlayerPrefs.DeleteAll();
        if(shouldLoad)
            LoadBindingsAndProcessors();
    }

    /// <summary>
    /// Saves the given action/binding pair to PlayerPrefs. The Action ID and binding index for the binding 
    /// being saved is stored in the key, while the control path to store (such as )
    /// is stored in the value.
    /// 
    /// </summary>
    /// <param name="action">The action we're saving the binding override for.</param>
    /// <param name="bindingIndex">The index of the binding we're overriding on the action.</param>
    /// <param name="path">
    /// A full control path from the root path (such as <Gamepad>/buttonSouth) to apply as an override
    /// to the given binding.
    /// </param>
    public static void SaveBinding(InputAction action, int bindingIndex, string path)
    {
        string actionBindingKey = getBindingPrefsKey(action, bindingIndex);

        PlayerPrefs.SetString(actionBindingKey, path);
    }


    /// <summary>
    /// Save the changes for the given processor to the saved processors list for the given action/binding pair.
    /// If The list doesn't exist yet, the list will be saved with only the given processor.
    /// 
    /// If the list exists but the processor is not already in the list, the new processor will be appended 
    /// to the end of the list.
    /// 
    /// If the given processor is already a part of the saved list, the arguments for the processor will 
    /// be updated and saved.
    /// </summary>
    /// <param name="action">The action that has the binding the processor will be saved for.</param>
    /// <param name="bindingIndex">The index in the action bindings list of the binding to save the processor for.</param>
    /// <param name="processorName">The name of the processor to be saved.</param>
    /// <param name="arguments">The arguments to be saved for the given processor.</param>
    /// <param name="appendToStart">Whether to append the given processor to the start or end of the list. </param>
    public static void SaveProcessor(InputAction action, int bindingIndex, string processorName, string arguments, bool appendToStart)
    {
        string processorKeyName = getProcessorPrefsKey(action, bindingIndex);

        string newProcessorString = processorName + "(" + arguments + ")";

        if (PlayerPrefs.HasKey(processorKeyName) && PlayerPrefs.GetString(processorKeyName).Length > 0)
        {
            // get the processors from prefs
            string processors = PlayerPrefs.GetString(processorKeyName);
            string updatedProcessors = "";

            if (ProcessorStringHelper.hasProcessor(processors, processorName))
            {
                // if we have a value for this processor saved, just update the argument for it
                updatedProcessors = ProcessorStringHelper.ChangeProcessorArguments(processors, processorName, arguments);
            }
            else
            {

                // if we don't have a value for the given processor name saved, append it to either the start or end of our processors string
                if (appendToStart)
                {
                    updatedProcessors = newProcessorString + "," + processors;
                }
                else
                {
                    updatedProcessors = processors + "," + newProcessorString;
                }
                
            }

            PlayerPrefs.SetString(processorKeyName, updatedProcessors);
        }
        else
        {
            PlayerPrefs.SetString(processorKeyName, newProcessorString);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Checks PlayerPrefs to see if any processors are saved for the given action/binding pair.
    /// If none exists, creates and saves processors for safety reasons.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    /// <returns></returns>
    public static bool BindingHasSavedProcessors(InputAction action, int bindingIndex)
    {
        bool processorsExist = PlayerPrefs.HasKey(getProcessorPrefsKey(action, bindingIndex));

        if (!processorsExist)
            createAndSaveEmptyProcessors(action, bindingIndex);

        return processorsExist;
    }

    /// <summary>
    /// Checks PlayerPrefs to see if a given processor (such as InvertVector2) is saved for the given
    /// action/binding pair. Creates and saves processors if none exist (for safety reasons).
    /// </summary>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    /// <param name="processorName"></param>
    /// <returns></returns>
    public static bool ProcessorIsSavedForBinding(InputAction action, int bindingIndex, string processorName)
    {
        if (!BindingHasSavedProcessors(action, bindingIndex))
        {
            return false;
        }
        else
        {
            string processors = PlayerPrefs.GetString(getProcessorPrefsKey(action, bindingIndex));
            return ProcessorStringHelper.hasProcessor(processors, processorName);
        }
    }

    /// <summary>
    /// Gets the currently saved processors for the given action/binding pair. If none exist, 
    /// an empty string will be saved and then returned.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    /// <returns></returns>
    public static string GetSavedProcessors(InputAction action, int bindingIndex)
    {
        string processorPrefsKey = getProcessorPrefsKey(action, bindingIndex);

        if (!BindingHasSavedProcessors(action, bindingIndex))
        {
            // create and save empty processors if none are saved
            createAndSaveEmptyProcessors(action, bindingIndex);
            return "";
        }
        else
        {
            return PlayerPrefs.GetString(processorPrefsKey);
        }
    }

    private static void createAndSaveEmptyProcessors(InputAction action, int bindingIndex)
    {
        string processorPrefsKey = getProcessorPrefsKey(action, bindingIndex);

        PlayerPrefs.SetString(processorPrefsKey, "");
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loops through all actions in all action maps and attempts to override all their bindings with versions from PlayerPrefs.
    /// Note that in this current system, any changes to the number of bindings/the state of the action at all will likely invalidate what is saved in PlayerPrefs,
    /// and the load will not be successful.
    /// 
    /// If binding overrides are not found for an action/binding pair, there will be no error - the current bindings for that action will be used instead.
    /// </summary>
    public void LoadBindingsAndProcessors()
    {
        if (!actionAsset)
        {
            Debug.LogError("Error: No action asset has been passed to the BindingSaveLoader - load failed.");
            return;
        }

        // load all controls and bindings saved in prefs for all action maps
        foreach (InputActionMap map in actionAsset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    // get the Prefs keys for this action/binding pair
                    string bindingPrefsKey = getBindingPrefsKey(action, i);
                    string processorPrefsKey = getProcessorPrefsKey(action, i);

                    bool bindingSaved = PlayerPrefs.HasKey(bindingPrefsKey);
                    bool processorsSaved = PlayerPrefs.HasKey(processorPrefsKey);

                    // only apply an override if something is saved
                    if(bindingSaved || processorsSaved)
                    {
                        string bindingOverride = action.bindings[i].effectivePath;
                        string processors = "";

                        // check if we have an override saved for this binding
                        if (bindingSaved)
                        {
                            bindingOverride = PlayerPrefs.GetString(bindingPrefsKey);
                        }

                        // check if we have processors saved for this binding
                        if (processorsSaved)
                        {
                            processors = PlayerPrefs.GetString(processorPrefsKey);

                            Debug.Log("Loading processors " + processors);
                        }

                        // apply the given path and processors to the binding we're loading
                        overrideBinding(action, i, bindingOverride, processors);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates a uniique PlayerPrefs key from an action and a binding index. The key will
    /// be in the format "[Action ID],[Binding Index]"
    /// </summary>
    /// <remarks>
    /// This key will likely be invalidated if bindings are added to or removed from the action 
    /// before runtime, as that may change the binding index of the specific binding that was 
    /// overriden in <see cref="SaveBinding(InputAction, int, string)"/>.
    /// </remarks>
    /// <param name="action">The action to get the ID from.</param>
    /// <param name="bindingIndex">The index of the binding we're overriding on the action.</param>
    /// <returns>A string to be used as a key in PlayerPrefs</returns>
    public static string getBindingPrefsKey(InputAction action, int bindingIndex)
    {
        return action.id.ToString() + "," + bindingIndex.ToString();
    }
    
    public static string getProcessorPrefsKey(InputAction action, int bindingIndex)
    {
         return getBindingPrefsKey(action, bindingIndex) + "," + "processors";
    }

    /// <summary>
    /// Overrides a given binding with the given control path. In essence, this is what "remaps" a binding to use a different 
    /// control when we load in the scene.
    /// </summary>
    /// <param name="action">The action we're appllying an override to.</param>
    /// <param name="bindingIndex">The index of the binding we're overriding on the action.</param>
    /// <param name="overridePath">
    /// A full control path from the root path (such as <Gamepad>/buttonSouth) to apply as an override
    /// to the given binding.
    /// </param>
    /// <param name="processors">
    /// A comma separated list of processors to add to the binding when we override it.
    /// </param>
    public static void overrideBinding(InputAction action, int bindingIndex, string overridePath, string processors)
    {
        InputBinding newBinding = new InputBinding(overridePath, processors: processors);
        InputActionRebindingExtensions.ApplyBindingOverride(action, bindingIndex, newBinding);
    }
}
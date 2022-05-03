using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Contains helpers to interface with processors, which are generally stored
/// as comma delimited strings.
/// 
/// <remarks>
/// Each processor is generally formatted as follows:
/// 
/// [Processor Name]([param]=[value] [delimiter - may be just a space, could include a comma] [param]=[value]...)
/// 
/// Additionally, multiple processors are stored in a comma delimited list on the individual actions/bindings they're applied 
/// to, making the final format something like:
/// 
/// [Processor String], [Processor String], ...
/// </remarks>
/// </summary>
public class ProcessorStringHelper : MonoBehaviour
{
    // delimiter for the processor list string. it's comma delimited, but 
    // commas can also be present in arguments, so we have to do this instead.
    private const string processorDelimRegex = @"(\)\s*,?)";

    // will match an individual processor string that has had it's last ), removed
    private const string processorStringRegex = @".+\(.*";

    /// <summary>
    /// Searches a processor string to see if it contains a given processor.
    /// </summary>
    /// <param name="processors">A comma-delimited string of processors.</param>
    /// <param name="processorName">The name of the processor to search for (case-sensitive).</param>
    /// <returns>True if the processor is found, false otherwise.</returns>
    public static bool hasProcessor(string processors, string processorName)
    {
        // if the name isn't in the string, the processor isn't present
        if (!processors.Contains(processorName))
        {
            return false;
        }

        // TODO: this works for now, but in the future we will want to make sure to check 
        // this with regex to actually make sure the given string is a processor name
        // instead of a value or parameter

        return true;
    }

    /// <summary>
    /// Retrieves the parameters to a given processor within a processor list string. The paramters
    /// are returned without parantheses.
    /// </summary>
    /// <param name="processors">A comma-delimited string of processors.</param>
    /// <param name="processorName">The name of the processor to get parameters for (case-sensitive)</param>
    /// <returns>The paramater string for the given processor.</returns>
    public static string GetProcessorArguments(string processors, string processorName)
    {
        if(!hasProcessor(processors, processorName))
        {
           throw new ArgumentException("The given processor is not in the given list");
        }

        List<string> processorStrings = GetProcessorStringList(processors);

        string arguments = "";

        foreach(string s in processorStrings)
        {
            if (s.Contains(processorName))
            {
                // get the substring between the ( and the ) in the processor string
                int startIndex = s.IndexOf('(') + 1;
                int endIndex = s.IndexOf(')');
                arguments = s.Substring(startIndex, endIndex - startIndex);
            }
        }

        return arguments;
    }

    /// <summary>
    /// Splits a processor list string into a list of individual processor strings.
    /// </summary>
    /// <param name="processors">A comma-delimited list of processors</param>
    /// <returns>A list of strings where the items are individual processor strings.</returns>
    public static List<string> GetProcessorStringList(string processors)
    {
        // TODO: pretty positive there's a way to do this w/o looping
        // I can't just split on the , because commas can be within the arguments in processors
        List<string> processorStrings = new List<string>();

        // this will remove the ) and , for each processor
        string[] splitString = Regex.Split(processors, processorDelimRegex);

        // get the actual processor strings we want, with the ending ) added back in
        foreach (string s in splitString)
        {
            if (Regex.Matches(s, processorStringRegex).Count > 0)
            {
                processorStrings.Add(s + ")");
            }
        }

        return processorStrings;
    }

    
    /// <summary>
    /// Changes the arguments to a given processor within a processor list string to a given value.
    /// </summary>
    /// <param name="processors">A comma-delimited list of processor strings</param>
    /// <param name="processorName">The name of the processor to modify the arguments for</param>
    /// <param name="arguments">The argument string to insert for the given processor (do not include parentheses)</param>
    /// <returns>The original processor list string with the desired arguments applied.</returns>
    public static string ChangeProcessorArguments(string processors, string processorName, string arguments)
    {
        if (!hasProcessor(processors, processorName))
        {
            throw new ArgumentException("The given processor is not in the given list");
        }

        List<string> processorStrings = GetProcessorStringList(processors);
        StringBuilder sb = new StringBuilder();
        
        // append all the individual processor strings, changing the arguments only on
        // the specific processor that was requested
        for(int i = 0; i < processorStrings.Count; i++)
        {
            // don't add a comma for the last item
            string commaString = (i == processorStrings.Count - 1) ? "" : ",";

            if (processorStrings[i].Contains(processorName))
            {
                // this is the string to modify
                sb.Append(processorName + "(" + arguments + ")" + commaString);
            }
            else
            {
                sb.Append(processorStrings[i] + commaString);
            }
        }

        return sb.ToString();
    }
}

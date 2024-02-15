using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditorInternal.VersionControl;

public class MultiStateAutomataUI : MonoBehaviour
{
    public MultipleStateAutomataManager manager;
    public CustomRule newRule;
    public TMP_InputField nieghborAmtRuleTriggerInputfield;
    public TMP_InputField neighborStatesToTriggerRule;
    public TMP_Dropdown originalStateDropdown;
    public TMP_Dropdown targetStateDropdown;

    public void CreateRule()
    {
        newRule = new CustomRule{
            NeighborCountsToTriggerRule = ConvertStringToIntArray(nieghborAmtRuleTriggerInputfield.text),
            NeighborStatesToTriggerRule = ConvertStringToIntArray(neighborStatesToTriggerRule.text),
            OriginalState = (CellState)originalStateDropdown.value,
            TargetState = (CellState)targetStateDropdown.value
        };

        manager.customRules.Add(newRule);
    }

    private void Start() {
        string[] cellStateContents = Enum.GetNames(typeof(CellState));
        List<string> contents = new List<string>(cellStateContents);
        originalStateDropdown.AddOptions(contents);
        targetStateDropdown.AddOptions(contents);
    }

    public int[] ConvertStringToIntArray(string stringToConvert)
    {
        string[] neighborAmtStringArray = stringToConvert.Split("n", StringSplitOptions.RemoveEmptyEntries);
        int[] neighborAmtIntArray = new int[neighborAmtStringArray.Length];
        for (int i = 0; i < neighborAmtStringArray.Length; i++)
        {
            int.TryParse(neighborAmtStringArray[i], out int parsedInt);
            neighborAmtIntArray[i] = parsedInt;
        }

        return neighborAmtIntArray;
    }
}

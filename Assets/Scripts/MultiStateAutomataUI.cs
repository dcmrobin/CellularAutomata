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
    public GameObject rulePrefab;
    public TMP_InputField nieghborAmtRuleTriggerInputfield;
    public TMP_InputField neighborStatesToTriggerRule;
    public TMP_Dropdown originalStateDropdown;
    public TMP_Dropdown targetStateDropdown;

    public GameObject viewRuleUI;

    public void CreateRule()
    {
        newRule = new CustomRule{
            NeighborCountsToTriggerRule = ConvertStringToIntArray(nieghborAmtRuleTriggerInputfield.text),
            NeighborStatesToTriggerRule = ConvertStringToIntArray(neighborStatesToTriggerRule.text),
            OriginalState = (CellState)originalStateDropdown.value,
            TargetState = (CellState)targetStateDropdown.value
        };

        manager.customRules.Add(newRule);

        GameObject newRuleGameObject = Instantiate(rulePrefab, transform.Find("Viewport").Find("Content"));
        newRuleGameObject.name = manager.customRules.Count.ToString();
        newRuleGameObject.transform.Find("Text").GetComponent<TMP_Text>().text = "Rule " + manager.customRules.Count;
        newRuleGameObject.transform.Find("deleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteRule(newRuleGameObject.transform.GetSiblingIndex()));
        newRuleGameObject.GetComponent<Button>().onClick.AddListener(() => ViewRule(1234, 1234, newRule.OriginalState.ToString(), newRule.TargetState.ToString(), Convert.ToInt32(newRuleGameObject.name)));
    }

    public void DeleteRule(int n)
    {
        manager.customRules.RemoveAt(n);
        for (int i = 0; i < transform.Find("Viewport").Find("Content").childCount; i++)
        {
            if (!transform.Find("Viewport").Find("Content").GetChild(i).gameObject.activeSelf)
            {
                Destroy(transform.Find("Viewport").Find("Content").GetChild(i).gameObject);
            }
        }
    }

    public void ViewRule(int nStates, int nAmts, string oState, string tState, int ruleNum)
    {
        viewRuleUI.SetActive(true);

        viewRuleUI.transform.Find("NeighborStatesTriggerRuleInputField").GetComponent<TMP_InputField>().text = nStates.ToString();
        viewRuleUI.transform.Find("NeighborAmtsTriggerRuleInputField").GetComponent<TMP_InputField>().text = nAmts.ToString();
        viewRuleUI.transform.Find("OriginalState").GetComponent<TMP_InputField>().text = oState;
        viewRuleUI.transform.Find("TargetState").GetComponent<TMP_InputField>().text = tState;
        viewRuleUI.transform.Find("Header").GetComponent<TMP_Text>().text = "Rule " + ruleNum.ToString();
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

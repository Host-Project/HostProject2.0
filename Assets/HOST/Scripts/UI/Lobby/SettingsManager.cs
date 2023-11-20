using HOST.Monitoring;
using HOST.Monitoring.Settings;
using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{

    [SerializeField]
    private ScenarioSettings defaultSettings;

    [SerializeField]
    private TMP_InputField expectedDurationInput;

    [SerializeField]
    private TMP_InputField onTimeToleranceInput;

    [SerializeField]
    private TMP_InputField hintsFrequencyInput;

    [SerializeField]
    private TMP_InputField pertubatorsFrequencyInput;

    [SerializeField]
    private HorizontalSelector riddleSelector;

    [SerializeField]
    private GameObject elementsList;
    [SerializeField]
    private GameObject elementUIPrefab;

    private ScenarioManager scenarioManager;

    private void Start()
    {
        scenarioManager = FindAnyObjectByType<ScenarioManager>();
        LoadScenarioSettings(defaultSettings);
        scenarioManager.SetScenarioSettings(defaultSettings);

    }
    public void LoadScenarioSettings(ScenarioSettings settings)
    {
        scenarioManager.SetScenarioSettings(settings);
        expectedDurationInput.onEndEdit.RemoveAllListeners();
        expectedDurationInput.text = settings.ExpectedDuration().ToString();
        expectedDurationInput.onEndEdit.AddListener((string value) => settings.SetExpectedDuration(float.Parse(value)));

        onTimeToleranceInput.onEndEdit.RemoveAllListeners();
        onTimeToleranceInput.text = (settings.DeltaOnTime * 100).ToString();
        onTimeToleranceInput.onEndEdit.AddListener((string value) => settings.DeltaOnTime = float.Parse(value) / 100);
        
        hintsFrequencyInput.onEndEdit.RemoveAllListeners();
        hintsFrequencyInput.text = settings.TimeBetweenHints.ToString();
        hintsFrequencyInput.onEndEdit.AddListener((string value) => settings.TimeBetweenHints = float.Parse(value));
        
        pertubatorsFrequencyInput.onEndEdit.RemoveAllListeners();
        pertubatorsFrequencyInput.text = settings.TimeBetweenPertubators.ToString();
        pertubatorsFrequencyInput.onEndEdit.AddListener((string value) => settings.TimeBetweenPertubators = float.Parse(value));

        PopulateRiddleSelector(settings);

    }

    public void PopulateRiddleSelector(ScenarioSettings settings)
    {
        riddleSelector.items.Clear();
        foreach (RiddleSettings riddle in settings.Riddles)
        {
            HorizontalSelector.Item item = new HorizontalSelector.Item();
            item.itemTitle = riddle.Riddle.name;
            item.onItemSelect.AddListener(() => PopulateElementsList(riddle));
            riddleSelector.items.Add(item);
        }

        PopulateElementsList(settings.Riddles[0]);

        riddleSelector.index = riddleSelector.defaultIndex;
        riddleSelector.SetupSelector();
    }

    public void PopulateElementsList(RiddleSettings settings)
    {
        foreach(Transform child in elementsList.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (WeightedElements element in settings.Elements)
        {
            GameObject elementUI = Instantiate(elementUIPrefab, elementsList.transform);
            elementUI.GetComponent<ElementsUI>().SetElementName(element.element.name);
            elementUI.GetComponent<ElementsUI>().SetElementInput(element.weight.ToString());
            elementUI.GetComponent<ElementsUI>().GetInputField().onEndEdit.AddListener((string value) => element.weight = int.Parse(value));
        }
    }

   
}

using TMPro;
using UnityEngine;

public class RiddleUI : MonoBehaviour
{
    
    [SerializeField]
    TMP_Text riddleName;

    [SerializeField]
    TMP_Text duration;

    [SerializeField]
    TMP_Text expectedDuration;
    public TMP_Text RiddleName { get => riddleName; set => riddleName = value; }
    public TMP_Text Duration { get => duration; set => duration = value; }
    public TMP_Text ExpectedDuration { get => expectedDuration; set => expectedDuration = value; }
}

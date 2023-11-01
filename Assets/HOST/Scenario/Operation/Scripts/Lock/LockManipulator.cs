using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class LockManipulator : MonoBehaviour
{

    public string Combination;

    public bool EnableManipulation = false;

    public bool IsOpen = false;


    public Transform[] Spinner = new Transform[3];

    public GameObject[] Buttons;

    public Transform Shackle;

    private Vector3Int _currentCode;

    public void Start()
    {
        UpdateRotation(0);
        UpdateRotation(1);
        UpdateRotation(2);

        ShowManipulationElements(EnableManipulation);
    }

    public UnityEvent OnUnlocked;

    public void PlayOpenAnimation()
    {
        Debug.Log("Open");
        Sequence sequence = DOTween.Sequence();
        sequence.Append(Shackle.DOLocalMoveY(0.0036f, 0.5f));
        sequence.Append(Shackle.DOLocalRotate(new Vector3(0f, 170f, 0f), 1f));
        IsOpen = true;
    }

    public void PlayCloseAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(Shackle.DOLocalRotate(new Vector3(0f, 0f, 0f), 1.5f));
        sequence.Append(Shackle.DOLocalMoveY(0f, 0.5f));
        IsOpen = false;
    }

    public void CheckCode()
    {
        if (Combination[0] == _currentCode.x && Combination[1] == _currentCode.y && Combination[2] == _currentCode.z)
        {
            PlayOpenAnimation();
            OnUnlocked.Invoke();
        }
        else if (IsOpen == true)
        {
            PlayCloseAnimation();
        }
    }

    public void ShowManipulationElements(bool show)
    {
        if (Buttons != null)
        {
            foreach (var button in Buttons)
            {
                button.SetActive(show);
            }
        }
    }

    public void NextLockIndex(int spinner)
    {
        if (!EnableManipulation)
        {
            return;
        }

        _currentCode[spinner] += 1;

        if (_currentCode[spinner] > 9)
        {
            _currentCode[spinner] = 0;
        }

        UpdateRotation(spinner);
        CheckCode();
    }

    public void PreviousLockIndex(int spinner)
    {
        if (!EnableManipulation)
        {
            return;
        }

        _currentCode[spinner] -= 1;

        if (_currentCode[spinner] < 0)
        {
            _currentCode[spinner] = 9;
        }

        UpdateRotation(spinner);
        CheckCode();
    }

    public void UpdateRotation(int spinner)
    {
        // 0 number is at 106° angle, each number is 36°
        float rotation = _currentCode[spinner] * 36 + 106;

        Spinner[spinner].DOLocalRotate(new Vector3(0f, rotation, 0f), 0.5f, RotateMode.Fast);
    }
}

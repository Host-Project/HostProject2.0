using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DigitSpinner : MonoBehaviour
{
    [SerializeField]
    private int currentNumber = 1;

    public UnityEvent OnDigitChanged;

    private float _sensitivity = 0.4f;
    private Vector3 _mouseReference;
    private Vector3 _mouseOffset;
    private Vector3 _rotation = Vector3.zero;
    private bool _isRotating;


    private bool hasMoved = false;
    private Vector3 oldRotation;

    private void Start()
    {
        UpdateRotation();
    }

    void Update()
    {
        if (transform.gameObject.TryGetComponent(out Rigidbody rb))
        {
            if (hasMoved && rb.angularVelocity.magnitude == 0f)
            {
                SetNumberFromAngle();
                hasMoved = false;
            }
            else if (rb.angularVelocity.magnitude != 0f)
            {
                hasMoved = true;
            }

            oldRotation = transform.rotation.eulerAngles;
        }

        if (_isRotating)
        {
            // offset
            _mouseOffset = (Input.mousePosition - _mouseReference);

            // apply rotation
            _rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;

            // rotate
            transform.Rotate(_rotation);

            // store mouse
            _mouseReference = Input.mousePosition;
        }

    }


    void OnMouseDown()
    {
        // rotating flag
        _isRotating = true;

        // store mouse
        _mouseReference = Input.mousePosition;
    }

    void OnMouseUp()
    {
        // rotating flag
        _isRotating = false;

        SetNumberFromAngle();
    }
    private void SetNumberFromAngle()
    {
        // 0 number is at 106° angle, each number is 36°
        float angle = transform.localEulerAngles.y;

        currentNumber = (Mathf.RoundToInt((angle - 106) / 36) % 10 + 10) % 10;
        OnDigitChanged.Invoke();
        UpdateRotation();
    }

    public int GetCurrentNumber()
    {
        return currentNumber;
    }

    private void UpdateRotation()
    {
        // 0 number is at 106° angle, each number is 36°
        float angle = 106 + currentNumber * 36;
        transform.DOLocalRotate(new Vector3(0f, angle, 0f), 0.5f, RotateMode.Fast);
    }
}

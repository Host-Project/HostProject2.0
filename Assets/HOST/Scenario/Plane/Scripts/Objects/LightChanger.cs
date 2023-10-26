using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChanger : MonoBehaviour
{
    public List<Color> LightColors;

    public Material LightMaterial;

    private int _lightIndex = 0;

    public float ChangeSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ChangeLight", 0f, ChangeSpeed);
    }

    private void ChangeLight()
    {
        _lightIndex++;

        if(_lightIndex >= LightColors.Count)
        {
            _lightIndex = 0;
        }

        LightMaterial.color = LightColors[_lightIndex];
        LightMaterial.SetColor("_EmissiveColor", LightColors[_lightIndex]);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingLight : MonoBehaviour
{
    // Start is called before the first frame update

    private bool isFlickering = false;

    [SerializeField]
    private Light _light;

    [SerializeField]
    private float timeDelay;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFlickering)
        {
            StartCoroutine(FlickeringLight());
        }
    }

    private void OnEnable()
    {
        _light.enabled = true;
        isFlickering = false;
    }

    private IEnumerator FlickeringLight()
    {
        isFlickering = true;
        _light.enabled = true;
        yield return new WaitForSeconds(timeDelay);
        _light.enabled = false;
        yield return new WaitForSeconds(timeDelay);
        isFlickering = false;
    }

  
}

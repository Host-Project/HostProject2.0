using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projector : MonoBehaviour
{

    public AudioSource AnnoucementSound;

    public Light projectLight;

    public List<RotatingProjector> rotators;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartProjecting()
    {
        AnnoucementSound.Play();
        projectLight.enabled = true;
        rotators.ForEach(r => r.IsRotating = true);
    }

    public void StopProjecting()
    {
        AnnoucementSound.Stop();
        projectLight.enabled = false;
        rotators.ForEach(r => r.IsRotating = false);
    }
}

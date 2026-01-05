using UnityEngine;
using UnityEngine.UI;

public class obj_instance : MonoBehaviour {

    public int number;       
    public int passwordIndex;
    public Text textobj;

    [Header("Beacon")]
    public ParticleSystem beaconPS;

    private void Awake()
    {
        if (beaconPS != null)
            beaconPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

   public void ShowBeacon()
    {
        Debug.Log("ShowBeacon: " + gameObject.name);
        if (beaconPS != null)
            beaconPS.Play();
        else
            Debug.LogWarning("Beacon PS NULL en " + gameObject.name);
    }


    public void HideBeacon()
    {
        if (beaconPS != null)
            beaconPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
   
}

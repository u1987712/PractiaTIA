using UnityEngine;
using UnityEngine.UI;

public class obj_instance : MonoBehaviour
{
    public int number;
    public int passwordIndex;
    public Text textobj;

    [Header("Spectral Reveal Shader")]
    public Material spectralMat;

    private void Awake()
    {
        if (spectralMat != null)
            spectralMat.SetFloat("_active", 0f);
    }

    public void ShowBeacon()
    {
        Debug.Log("ShowBeacon: " + gameObject.name);

        if (spectralMat != null)
            spectralMat.SetFloat("_active", 1f);
        else
            Debug.LogWarning("Material NULL en " + gameObject.name);
    }

    public void HideBeacon()
    {
        if (spectralMat != null)
            spectralMat.SetFloat("_active", 0f);
    }
}

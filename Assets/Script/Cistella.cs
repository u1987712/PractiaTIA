using TMPro;
using UnityEngine;

public class Cistella : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public GameObject Pla;
    private string punts;
    private int puntFet;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Entra"))
        {
            puntFet = puntFet + 1;
            punts = puntFet.ToString();
            Text.text = punts;
        }
    }
}

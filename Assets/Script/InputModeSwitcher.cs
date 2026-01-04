using UnityEngine;

public class InputModeSwitcher : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;
    public GameObject leftHand;
    public GameObject rightHand;

    private bool handsActive = false;

    public void ToggleInputMode() {
        handsActive = !handsActive;

        leftController.SetActive(!handsActive);
        rightController.SetActive(!handsActive);

        leftHand.SetActive(handsActive);
        rightHand.SetActive(handsActive);
    }
}

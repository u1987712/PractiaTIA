using UnityEngine;

public class KeypadUI : MonoBehaviour {
    public string input = "";
    public GameManager gameManager;

    public void PressNumber(int n) {
        input += n.ToString();
    }

    public void PressOK() {
        if (gameManager.CheckPassword(input)) {
            gameManager.EndGameSuccess();
        } else {
            Debug.Log("Incorrecto");
            input = "";
        }
    }
}

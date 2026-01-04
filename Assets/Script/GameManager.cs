using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField] private XROrigin xrOrigin;

    [Header("UI")]
    public GameObject mainMenu;
    public GameObject bookHUD;
    public Text TextPassword;

    [Header("Spawn")]
    public GameObject objectsToSpawn; // prefabs a instanciar
    public Terrain terrain; // terrain
    public int spawnMaskLayerIndex = 1; // índice del layer que marca zona de spawn
    public int numObjSpawn = 4;
    public int getObjSpawn = 0;
    public float minDistanceBetweenObjects = 2f;
     public Text TextNumObj;
    
    [Header("Password")]
    public string passwordSequence = "";   

    [Header("Timer")]
    public Text timerText;
    public float gameDuration = 30f; // en segundos
    private float timer;
    private bool gameRunning = false;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        // Inicio: menú activo, HUD apagado
        mainMenu.SetActive(true);
        bookHUD.SetActive(false);
    }

    public void StartGame() {
        // Ocultar menú
        mainMenu.SetActive(false);

        // Activar libro
        bookHUD.SetActive(true);

        // Spawn inicial de objetos
        SpawnObjectsRandomly();

        // Empezar temporizador
        timer = gameDuration;
        gameRunning = true;

        TextNumObj.text = string.Format("{0}/{1}", 0, numObjSpawn + 1);
    }

    void Update() {
        if (!gameRunning) return;

        // Actualizar temporizador
        timer -= Time.deltaTime;
        UpdateTimerOnHUD(timer);

        if (timer <= 0f) {
            EndGame();
        }
    }

    void EndGame() {
        gameRunning = false;
        Debug.Log("Juego terminado, reiniciamos!");
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log(currentScene.name);
        SceneManager.LoadScene(currentScene.name);
    }

    void UpdateTimerOnHUD(float t){
        if (timerText == null) return;

        // Asegurarnos de que el tiempo no sea negativo
        timer = Mathf.Max(timer, 0f);

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
    }

    public bool CheckPassword(string input) {
        return input == passwordSequence;
    }

    public void EndGameSuccess() {
        gameRunning = false;
        Debug.Log("CONTRASEÑA CORRECTA");
        
        // añadir funcionalizad de aparecer texto en popup de ganadar y boton de reset

     
    }


    void SpawnObjectsRandomly() {
        passwordSequence = "";

        for (int i = 0; i < numObjSpawn; i++) {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.zero) {
                GameObject obj = Instantiate(objectsToSpawn, spawnPos, Quaternion.identity);
                obj_instance co = obj.GetComponent<obj_instance>();
                co.number = Random.Range(1, 10);
                co.passwordIndex = i+1;
                co.textobj.text = string.Format("{0}-[{1}]",co.number, co.passwordIndex);

                spawnedObjects.Add(obj);
                passwordSequence += co.number.ToString();
            }
        }
    }

    Vector3 GetValidSpawnPosition() {
        Vector3 pos = Vector3.zero;
        bool valid;
        int attempts = 0;

        do {
            attempts++;

            // Posición aleatoria sobre el terrain
            float x = Random.Range(terrain.transform.position.x, terrain.transform.position.x + terrain.terrainData.size.x);
            float z = Random.Range(terrain.transform.position.z, terrain.transform.position.z + terrain.terrainData.size.z);
            pos = new Vector3(x, 0, z);
            pos.y = terrain.SampleHeight(pos);

            valid = true;

            // Comprobar si la máscara (Layer 1) domina la posición
            float normX = (pos.x - terrain.transform.position.x) / terrain.terrainData.size.x;
            float normZ = (pos.z - terrain.transform.position.z) / terrain.terrainData.size.z;

            int mapX = Mathf.RoundToInt(normX * (terrain.terrainData.alphamapWidth - 1));
            int mapZ = Mathf.RoundToInt(normZ * (terrain.terrainData.alphamapHeight - 1));

            float[,,] splatmap = terrain.terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            float textureValue = splatmap[0, 0, spawnMaskLayerIndex];

            if (textureValue < 0.5f) valid = false; // si la máscara no domina --> no spawn

            // Evitar solapamiento con otros objetos
            foreach (var existing in spawnedObjects) {
                if (Vector3.Distance(existing.transform.position, pos) < minDistanceBetweenObjects) {
                    valid = false;
                    break;
                }
            }

        } while (!valid && attempts < 100);

        if (!valid) return Vector3.zero; // si no encontró posición válida
        return pos;
    }


    public void ExitGame() {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void ObjectDroppedCorrectly(GameObject obj, float bonusTime) {
        if (spawnedObjects.Contains(obj)) {
            spawnedObjects.Remove(obj);
            obj_instance co = obj.GetComponent<obj_instance>();
            
            timer += bonusTime; // sumamos tiempo al temporizador
            getObjSpawn++;
            TextNumObj.text = string.Format("{0}/{1}", getObjSpawn, numObjSpawn + 1);
            TextPassword.text += string.Format("{0}-[{1}] ", co.number, co.passwordIndex);
            
            
            Destroy(obj); // eliminamos el objeto
            Debug.Log("Objeto depositado correctamente. Tiempo añadido: " + bonusTime);
        }
    }

}

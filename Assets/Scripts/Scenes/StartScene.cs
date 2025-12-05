using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public GameObject Bot;
    public float spawnInterval = 3f;
    private float timer;

    private Vector3 spawnBasePos = new(-228f, .5f, 63f);

    void Start()
    {
        // Initialize timer when scene loads
        timer = Random.Range(0f, spawnInterval);
        
        // Add controller menu navigation if not already present
        if (FindObjectOfType<ControllerMenuNavigation>() == null)
        {
            gameObject.AddComponent<ControllerMenuNavigation>();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnBot();
            timer = Random.Range(spawnInterval - 2.9f, spawnInterval + 1f);
        }
    }

    void SpawnBot()
    {
        if (Bot != null)
        {
            float zOffset = Random.Range(-3f, 3f);
            Vector3 spawnPos = new(spawnBasePos.x, spawnBasePos.y, spawnBasePos.z + zOffset);

            Quaternion spawnRot = Quaternion.Euler(0f, 90f, 0f);

            Instantiate(Bot, spawnPos, spawnRot);
        }
    }

    public void SinglePlayer()
    {
        TrackGen.Scene2Load = "SinglePlayer";
        SceneManager.LoadScene("LoadingScene");
    }

    public void MultiPlayer()
    {
        TrackGen.Scene2Load = "MultiPlayer";
        SceneManager.LoadScene("LoadingScene");
    }
}

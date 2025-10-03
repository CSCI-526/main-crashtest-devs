using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public GameObject Bot;
    public float spawnInterval = 3f;
    private float timer;

    private Vector3 spawnBasePos = new(-228f, .5f, 63f);

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
        SceneManager.LoadScene("Assets/Scenes/SinglePlayer.unity");
    }

    public void MultiPlayer()
    {
        SceneManager.LoadScene("Assets/Scenes/MultiPlayer.unity");
    }
}

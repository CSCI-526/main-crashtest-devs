using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StartSceneBot : MonoBehaviour
{
    private const float speed = 160;

    private Rigidbody rb;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(UnityEngine.Random.Range(speed - 10f, speed + 10f), 0f, 0f);
    }
}

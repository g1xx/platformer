using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Settings")]
    public float amplitude = 0.3f; 
    public float frequency = 1.5f; 

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
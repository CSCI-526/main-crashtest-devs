using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    public static System.Action<Transform, string> OnAnyPlaneTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnAnyPlaneTrigger?.Invoke(transform.parent, other.gameObject.name);
    }
}

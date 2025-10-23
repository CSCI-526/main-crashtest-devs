using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    public static System.Action<Transform, string, int> OnAnyPlaneTrigger;
    public int cpNum;

    private void OnTriggerEnter(Collider other)
    {
        OnAnyPlaneTrigger?.Invoke(transform.parent.parent, other.gameObject.name, cpNum);
    }
}

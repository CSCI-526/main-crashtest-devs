
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public static class BotPlayer
{
    public static (int wheelsInContact, RoadMesh roadMesh) IsGrounded(GameObject player, float groundCheckDistance, LayerMask roadLayer)
    {
        int wheelsInContact = 0;
        RoadMesh roadMesh = null;

        Transform corners = player.transform.Find("corners");

        for (int i = 0; i < corners.childCount; i++)
        {
            Transform wheel = corners.GetChild(i);

            if (Physics.CheckSphere(wheel.position, groundCheckDistance, roadLayer))
            {
                if (Physics.Raycast(wheel.position, Vector3.down, out RaycastHit hit, groundCheckDistance + 1f, roadLayer))
                {
                    if (hit.collider.TryGetComponent<RoadMesh>(out var rm))
                        roadMesh = rm;
                }
                wheelsInContact++;
            }
        }

        if (wheelsInContact == 0)
        {
            if (Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, .75f))
                if (!hit.collider.TryGetComponent<RoadMesh>(out var _)) wheelsInContact = -1;
        }

        return (wheelsInContact, roadMesh);
    }
}

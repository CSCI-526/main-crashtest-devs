
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public static class BotPlayer
{

    public static (int wheelsInContact, RoadMesh roadMesh) IsGrounded(GameObject player, float groundCheckDistance, LayerMask roadLayer)
    {
        int wheelsInContact = 4;
        RoadMesh roadMesh = null;

        for (int i = 0; i < 4; i++)
        {
            Transform wheel = player.transform.Find("corners").transform.GetChild(i);
            if (!Physics.Raycast(wheel.position, Vector3.down, out RaycastHit hit, groundCheckDistance, roadLayer)) wheelsInContact -= 1;
            else roadMesh = hit.collider.GetComponent<RoadMesh>();
        }

        return (wheelsInContact, roadMesh);
    }

}

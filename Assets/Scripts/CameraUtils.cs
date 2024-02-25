using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class CameraUtils
{
    public static float GetSmoothedCameraTargetHeight(Vector3 cameraTargetPosition, LayerMask layerMask)
    {
        float gridSize = 2.0f;
        float[,] heights = new float[2, 2];

        Vector3 cameraTargetPositionGrid =
            new Vector3(Mathf.Floor(cameraTargetPosition.x / gridSize), cameraTargetPosition.y / gridSize, Mathf.Floor(cameraTargetPosition.z / gridSize)) * gridSize;
        
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector3 samplePosition = new Vector3(x, 0, y) * gridSize;
                heights[x, y] = cameraTargetPosition.y;
                if (Physics.SphereCast(cameraTargetPositionGrid + samplePosition + Vector3.up * 200, gridSize*0.5f, Vector3.down, out var raycastHit, 9999, layerMask))
                {
                    heights[x, y] = raycastHit.point.y;
                    if (NavMesh.SamplePosition(raycastHit.point, out var navMeshHit, 5, ~0))
                    {
                        heights[x, y] = Mathf.Max(heights[x, y], navMeshHit.position.y);
                    }
                }
            }
        }

        float weightX = (cameraTargetPosition.x - cameraTargetPositionGrid.x) / gridSize;
        float weightZ = (cameraTargetPosition.z - cameraTargetPositionGrid.z) / gridSize;

        return Mathf.Lerp(
            Mathf.Lerp(heights[0, 0], heights[1, 0], weightX),
            Mathf.Lerp(heights[0, 1], heights[1, 1], weightX),
            weightZ
        );
    }
}

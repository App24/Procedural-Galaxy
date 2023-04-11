using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODManager : MonoBehaviour
{
    public int[] lodLevels;

    public static LODManager instance;

    private void Awake()
    {
        instance = this;
    }

    public int GetLODLevel(Vector3 position)
    {
        return GetLODLevel(Vector3.Distance(Camera.main.transform.position, position));
    }

    public int GetLODLevel(float distance)
    {
        for (int i = 0; i < lodLevels.Length; i++)
        {
            var lod = lodLevels[i];
            if (distance <= lod)
            {
                if (i == 0) return 0;
                return i - 1;
            }
        }
        return lodLevels.Length - 1;
    }
}

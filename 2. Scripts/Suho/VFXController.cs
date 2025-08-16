using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public static PoolableVFX InstantiateVFX(string poolID, GameObject prefab)
    {
        GameObject go = ObjectPoolManager.Instance.GetObject(poolID);
        if (go == null)
        {
            go = Instantiate(prefab);
            go.name = poolID;
        }

        PoolableVFX vfx = go.GetComponent<PoolableVFX>();
        vfx.particle.Stop();
        return vfx;
    }


    public static List<PoolableVFX> VFXListPlay(List<VFXData> vfxList, VFXType vfxType, VFXSpawnReference unit, IEffectProvider effectProvider, bool isAwakePlay)
    {
        if(vfxList == null) return null;
        List<PoolableVFX> returnVFX = new();
        foreach (VFXData vfxData in vfxList)
        {
            if (vfxData.reference != unit)
            {
                continue;
            }

            if (vfxData.type == vfxType)
            {
                PoolableVFX vfx = InstantiateVFX(vfxData.VFXPoolID, vfxData.VFXPrefab);
                if (vfx == null)
                {
                    continue;
                }

                vfx.SetData(vfxData, effectProvider);
                returnVFX.Add(vfx);
                if (isAwakePlay)
                {
                    vfx.OnSpawnFromPool();
                }
            }
        }

        return returnVFX;
    }


    public static List<PoolableVFX> VFXListPlayOnTransform(List<VFXData> vfxList, VFXType vfxType, GameObject effect)
    {
        if(vfxList == null) return null;
        List<PoolableVFX> returnVFX = new();
        foreach (VFXData vfxData in vfxList)
        {
            if (vfxData.type == vfxType)
            {
                PoolableVFX vfx = InstantiateVFX(vfxData.VFXPoolID, vfxData.VFXPrefab);
                returnVFX.Add(vfx);
                vfx.OnSpawnFromPool(effect);
            }
        }

        return returnVFX;
    }
}
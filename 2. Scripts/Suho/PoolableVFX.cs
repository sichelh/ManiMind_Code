using PixPlays.ElementalVFX;
using System;
using System.Collections;
using UnityEngine;

public class PoolableVFX : MonoBehaviour, IPoolObject
{
    [SerializeField]private string poolId;
    [SerializeField]private int poolSize;
    public ParticleSystem particle;
    public GameObject GameObject => gameObject;
    public string PoolID => poolId;
    public int PoolSize => poolSize;
    protected VFXData VFXData;
    public IEffectProvider VFXTarget { get; set; }
    
    public Action OnTrigger { get; set; }


    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    public IEnumerator PlayVFX()
    {
        particle.Play();
        yield return new WaitWhile(() => particle.IsAlive(true));
        RemoveVFX();
    }

    public IEnumerator PlayVFXNoReturn()
    {
        particle.Play();
        yield return new WaitWhile(() => particle.IsAlive(true));
        StopAllCoroutines();
    }

    public void PlayDotVFX()
    {
        StartCoroutine(PlayVFXNoReturn());
    }

    public void RemoveVFX()
    {
        StopAllCoroutines();
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    public void AdjustTransform()
    {
        float scaleY = VFXTarget.Collider.transform.localScale.y;
        if (scaleY < 1f)
        {
            scaleY = 1f;
        }
;
        
        switch (VFXData.bodyType)
        {
            case VFXBodyPartType.Core : transform.position = VFXTarget.Collider.bounds.center;
                break;
            case VFXBodyPartType.Head : transform.position = new Vector3(VFXTarget.Collider.bounds.center.x,VFXTarget.Collider.bounds.max.y,VFXTarget.Collider.bounds.center.z);
                break;
            case VFXBodyPartType.feet : transform.position = new Vector3(VFXTarget.Collider.bounds.center.x,VFXTarget.Collider.bounds.min.y,VFXTarget.Collider.bounds.center.z);
                break;
        }
        
        transform.rotation = VFXTarget.Collider.transform.rotation;
        if (VFXData.isParent == true)
        {
            transform.SetParent(VFXTarget.Collider.transform, true);
            transform.localPosition += VFXData.LocalPosition;
            transform.localRotation = Quaternion.Euler(VFXData.LocalRotation);
            transform.localScale =  VFXData.LocalScale * (1/scaleY);
        }
        else
        {
            // 월드 좌표 이동 대신 유닛(부모) 기준 로컬 좌표 이동
            transform.position = VFXTarget.Collider.transform.TransformPoint(
                VFXTarget.Collider.transform.InverseTransformPoint(transform.position) + VFXData.LocalPosition
            );

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles) * Quaternion.Euler(VFXData.LocalRotation);
            transform.localScale = VFXData.LocalScale;
        }
    }
    
    public void AdjustTransform(GameObject effect)
    {
        this.transform.position = effect.transform.position;
        this.transform.rotation = effect.transform.rotation;
        transform.parent = effect.transform;
    }
    
    
    
    public void SetData(VFXData data, IEffectProvider effectProvider)
    {
        VFXData = data;
        VFXTarget = effectProvider;
        
    }    
    
    public void SetData(VFXData data,IEffectProvider effectProvider, Action trigger)
    {
        VFXData = data;
        VFXTarget = effectProvider;
        
        if (trigger != null)
        {
            trigger += OnSpawnFromPool;
        }
    }

    public void OnSpawnFromPool(GameObject effect)
    {
        AdjustTransform(effect);
        // Debug.Log(pos.position + " "+ pos.rotation.eulerAngles);
        StartCoroutine(PlayVFX());
    }

    public void OnSpawnFromPool()
    {
        AdjustTransform();
        StartCoroutine(PlayVFX());
    }

    public void OnReturnToPool()
    {
    }


}

using UnityEngine;

[System.Serializable]
public class VFXData
{
    public GameObject VFXPrefab;
    public string VFXPoolID;
    
    [Header("LocalTransform 미세조정 값")] 
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale = Vector3.one;
    
    [Header("이펙트가 발생하는 콜라이더 위치")]
    public VFXSpawnReference reference;

    public bool isParent = false;
    
    [Header("이펙트가 발생할 타이밍")]
    public VFXType type;

    [Header("이펙트가 발생하는 유닛의 세부위치")] 
    public VFXBodyPartType bodyType; 

}
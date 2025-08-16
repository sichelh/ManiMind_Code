using UnityEngine;

public interface IEffectProvider
{
    public Collider Collider { get; }

    public abstract Vector3 GetCenter();
}

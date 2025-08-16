using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageNumbersPro;
using UnityEditor;


public enum DamageType
{
    Normal,
    Critical,
    Heal,
    Miss,
    Immune,
    Shield
}

public class DamageRequest
{
    public Vector2 AnchoredPos;
    public DamageType Type;
    public float Value;
    public string Text;

    public DamageRequest(Vector2 anchoredPos, DamageType type, float value = 0f, string text = null)
    {
        AnchoredPos = anchoredPos;
        Type = type;
        Value = value;
        Text = text;
    }
}

public class DamageFontManager : SceneOnlySingleton<DamageFontManager>
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private DamageNumber normalDamageNumber;
    [SerializeField] private DamageNumber criticalDamageNumber;
    [SerializeField] private DamageNumber healDamageNumber;
    [SerializeField] private DamageNumber missDamageNumber;
    [SerializeField] private DamageNumber immuneDamageNumber;
    [SerializeField] private DamageNumber shieldDamageNumber;
    [SerializeField] private Camera mainCamera;

    private Dictionary<DamageType, DamageNumber> damageNumberMap = new();


    private Queue<DamageRequest> damageQueue = new();
    private Coroutine damageFontCoroutine;

    protected override void Awake()
    {
        base.Awake();
        damageNumberMap = new Dictionary<DamageType, DamageNumber>
        {
            { DamageType.Normal, normalDamageNumber },
            { DamageType.Critical, criticalDamageNumber },
            { DamageType.Heal, healDamageNumber },
            { DamageType.Miss, missDamageNumber },
            { DamageType.Immune, immuneDamageNumber },
            { DamageType.Shield, shieldDamageNumber }
        };
    }

    public void SetDamageNumber(IDamageable target, float damage, DamageType damageType)
    {
        Vector3 worldPos  = target.Collider.transform.position + new Vector3(0, 1.5f, 0);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, // ✅ canvas 기준!
            screenPos,
            null,
            out Vector2 anchoredPos
        );
        string text = damageType switch
        {
            DamageType.Miss   => "MISS",
            DamageType.Immune => "IMMUNE",
            _                 => string.Empty
        };

        damageQueue.Enqueue(new DamageRequest(anchoredPos, damageType, damage, text));

        if (damageFontCoroutine == null)
        {
            damageFontCoroutine = StartCoroutine(PlayerDamageFont());
        }
    }

    private IEnumerator PlayerDamageFont()
    {
        while (damageQueue.Count > 0)
        {
            DamageRequest request = damageQueue.Dequeue();

            if (damageNumberMap.TryGetValue(request.Type, out DamageNumber damageNumber))
            {
                if (!string.IsNullOrEmpty(request.Text))
                {
                    damageNumber.SpawnGUI(rectTransform, request.AnchoredPos, request.Text);
                }
                else
                {
                    damageNumber.SpawnGUI(rectTransform, request.AnchoredPos, request.Value);
                }
            }

            yield return new WaitForSeconds(0.2f); // 텀 조절
        }

        damageFontCoroutine = null;
    }
}
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * RangeSkill을 사용할 경우에 투사체에 추가되는 컴포넌트
 * poolId로 오브젝트 풀링에 사용되는 id값 입력
 * poolSize는 미리 만들어놓을 오브젝트의 수 입력
 * projectileSpeed는 투사체의 속도조절
 * ProjectileInterpolationMode는 투사체의 발사가 어떻게 이루어지는지 조절
 * => 추후 메테오의 경우에는 타겟의 머리위에서 투사체가 시작되도록 Mode추가
 * EffectData는 스킬의 효과를 담아둔다.
 * direction은 투사체의 방향조절
 * StartPosiion은 투사체의 시작위치조절
 */
public class PoolableProjectile : MonoBehaviour, IPoolObject, IEffectProvider
{
    [SerializeField]
    private string poolId;

    [SerializeField]
    private int poolSize;

    [SerializeField]
    private float projectileSpeed;

    private SkillEffectData effectData;

    private float smoothTime = 0.3f;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 direction = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private float _delta = 0;
    public GameObject GameObject => gameObject;
    public string     PoolID     => poolId;
    public int        PoolSize   => poolSize;

    public IDamageable Target;


    public SkillEffectData EffectData      => effectData;
    public float           ProjectileSpeed => projectileSpeed;
    public Vector3         Direction       => direction;
    public Vector3         StartPosition   => startPosition;

    public bool isShooting = false;
    public ProjectileInterpolationMode mode;

    public ProjectileTrigger trigger;

    private IAttackable attacker;

    public Collider Collider => trigger.colider;


    private void Awake()
    {
        trigger = GetComponentInChildren<ProjectileTrigger>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (isShooting)
        {
            float step = ProjectileSpeed * Time.deltaTime;
            ShootProjectile(mode, step); // ← 매 프레임 이동량만 전달
        }
    }


    public void ShootProjectile(ProjectileInterpolationMode interpolationMode, float delta)
    {
        switch (interpolationMode)
        {
            case ProjectileInterpolationMode.Linear:
                transform.position += direction.normalized * delta;
                break;

            case ProjectileInterpolationMode.Lerp:
                transform.position = Vector3.Lerp(transform.position, direction, delta);
                break;

            case ProjectileInterpolationMode.MoveTowards:
                transform.position = Vector3.MoveTowards(transform.position, direction, delta);
                break;

            case ProjectileInterpolationMode.SmoothDamp:
                transform.position = Vector3.SmoothDamp(transform.position, direction, ref velocity, smoothTime);
                break;

            case ProjectileInterpolationMode.Slerp:
                Vector3 offset = Vector3.Slerp(startPosition, direction, delta);
                transform.position = direction + offset;
                break;
            case ProjectileInterpolationMode.Fall:
                transform.position = direction + (Vector3.up * 10);
                mode = ProjectileInterpolationMode.MoveTowards;
                break;
        }
    }

    public void Initialize(IAttackable attacker, SkillEffectData effect, Vector3 startPos, Vector3 dir, IDamageable target)
    {
        //기존 구독되어있던 이벤트 해제
        trigger.OnTriggerTarget -= HandleTrigger;
        this.attacker = attacker;
        effectData = effect;
        startPosition = startPos;
        direction = dir;
        gameObject.transform.position = startPosition;
        gameObject.transform.LookAt(target.Collider.transform);
        Target = target;
        trigger.target = Target;
        trigger.OnTriggerTarget += HandleTrigger;
        OnSpawnFromPool();
        if (attacker.SkillController?.CurrentSkillData?.skillSo?.skillTimeLine != null)
        {
            CameraManager.Instance.ChangeFollowTarget(this);
        }
    }

    public void Initialize(IAttackable attacker, Vector3 startPos, Vector3 dir, IDamageable target)
    {
        trigger.OnTriggerTarget -= HandleAttackTrigger;
        this.attacker = attacker;
        startPosition = startPos;
        direction = dir;
        gameObject.transform.position = startPosition;
        gameObject.transform.LookAt(dir);
        Target = target;
        trigger.target = Target;
        trigger.OnTriggerTarget += HandleAttackTrigger;
        OnSpawnFromPool();
    }

    public void OnSpawnFromPool()
    {
        _delta = 0;
        isShooting = true;
    }

    public void OnReturnToPool()
    {
        isShooting = false;
    }

    private void HandleTrigger()
    {
        effectData.AffectTargetWithSkill(attacker, Target as Unit);
        if (attacker is Unit unit)
        {
            unit.InvokeSkillFinished();
        }

        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    private void HandleAttackTrigger()
    {
        //감정별 대미지
        float finalValue = attacker.StatManager.GetValue(StatType.AttackPow);
        bool  isCritical = Random.value < attacker.StatManager.GetValue(StatType.CriticalRate);
        if (isCritical)
        {
            float critBouns = attacker.StatManager.GetValue(StatType.CriticalDam);
            finalValue *= 2.0f + critBouns;
        }


        float multiplier = EmotionAffinityManager.GetAffinityMultiplier(attacker.CurrentEmotion.EmotionType, Target.CurrentEmotion.EmotionType);
        Target.TakeDamage(finalValue * multiplier, StatModifierType.Base, isCritical);

        if (attacker is Unit unit)
        {
            unit.InvokeRangeAttackFinished();
        }

        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    public Vector3 GetCenter()
    {
        return transform.position;
    }
}
/// <summary>
/// 공격할때 발동 되는 패시브
/// </summary>
public interface IPassiveAttackTrigger
{
    void OnAttack(float value);
}

/// <summary>
/// 데미지를 입었을때 발동 되는 패시브
/// </summary>
public interface IPassiveDamageResponse
{
    void OnDamageReceived();
}

/// <summary>
/// 스택이 증가될때 발동되는 패시브
/// </summary>
public interface IPassiveEmotionStackTrigger
{
    void OnEmotionStackIncreased(BaseEmotion emotion);
}

/// <summary>
/// 턴이 시작될때 발동 되는 패시브
/// </summary>
public interface IPassiveTurnStartTrigger
{
    void OnTurnStart(Unit unit);
}

public interface IPassiveTurnEndTrigger
{
    void OnTurnEnd(Unit unit);
}

/// <summary>
/// 감정 데미지를 수정하는 패시브
/// </summary>
public interface IPassiveEmotionDamageModifier
{
    float ModifyEmotionDamage(float baseDamage);
}

/// <summary>
/// 죽으면 발동 되는 패시브
/// </summary>
public interface IPassiveOnDeathTrigger
{
    void OnDeath();
}

/// <summary>
/// 아군이 죽으면 발동 되는 패시브
/// </summary>
public interface IPassiveAllyDeathTrigger
{
    void OnAllyDead();
}

/// <summary>
/// 감정이 바뀔때 발동되는 패시브
/// </summary>
public interface IPassiveChangeEmotionTrigger
{
    void OnChangeEmotion();
}

/// <summary>
/// 감정의 디버프의 수치를 조절해주는 패시브
/// </summary>
public interface IPassiveEmotionDebuffReducer
{
    void OnDebuffReducer(ref float debuffValue);
}
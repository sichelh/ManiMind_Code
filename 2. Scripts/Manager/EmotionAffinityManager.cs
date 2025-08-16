using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EmotionAffinityManager
{
    private const float AdvantageBonus = 1.3f;
    private const float DisadvantagePenalty = 0.7f;

    public static float GetAffinityMultiplier(EmotionType attacker, EmotionType defender)
    {
        if (attacker == EmotionType.Neutral || defender == EmotionType.Neutral || attacker == defender)
        {
            return 1.0f;
        }

        if (IsAdvantage(attacker, defender))
            return AdvantageBonus;

        if (IsDisadvantage(attacker, defender))
            return DisadvantagePenalty;

        return 1.0f;
    }

    private static bool IsAdvantage(EmotionType attacker, EmotionType defender)
    {
        return (attacker == EmotionType.Depression && defender == EmotionType.Joy)
               || (attacker == EmotionType.Joy && defender == EmotionType.Anger)
               || (attacker == EmotionType.Anger && defender == EmotionType.Depression);
    }

    private static bool IsDisadvantage(EmotionType attacker, EmotionType defender)
    {
        return (attacker == EmotionType.Joy && defender == EmotionType.Depression)
               || (attacker == EmotionType.Anger && defender == EmotionType.Joy)
               || (attacker == EmotionType.Depression && defender == EmotionType.Anger);
    }
}
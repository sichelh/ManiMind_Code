using System;
using System.Collections;
using UnityEngine;

public class MeleeCombatAction : ICombatAction
{
    public event Action OnActionComplete;
    private bool isTimeLinePlaying;

    public void Execute(Unit attacker)
    {
        TimeLineManager.Instance.PlayTimeLine(CameraManager.Instance.cinemachineBrain, CameraManager.Instance.skillCameraController, attacker, out isTimeLinePlaying);
        attacker.ExecuteCoroutine(WaitForAnimationDone(attacker));
    }

    private IEnumerator WaitForAnimationDone(Unit attacker)
    {
        if (isTimeLinePlaying)
        {
            yield return new WaitUntil(() => !attacker.IsTimeLinePlaying);
            OnActionComplete?.Invoke();
        }
    }
}
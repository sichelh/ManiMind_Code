using System;
using UnityEngine;

namespace EnemyState
{
    public class HitState : IState<EnemyUnitController, EnemyUnitState>
    {
        private readonly int isHit = Define.HitAnimationHash;
        private bool canCounter;
        private Action onHitFinishedHandler;

        public void OnEnter(EnemyUnitController owner)
        {
            owner.IsAnimationDone = false;
            owner.Animator.SetTrigger(isHit);
            owner.PlayHitVoiceSound();
            canCounter = owner.CanCounterAttack(owner.LastAttacker);
            if (owner.LastAttacker == null)
            {
                return;
            }

            if (canCounter)
            {
                onHitFinishedHandler = () => owner.StartCounterAttack(owner.LastAttacker);
            }
            else if (!owner.LastAttacker.SkillController.CurrentSkillData?.skillSo.skillTimeLine)
            {
                onHitFinishedHandler = owner.LastAttacker.InvokeHitFinished;
            }

            if (onHitFinishedHandler != null)
            {
                owner.OnHitFinished += onHitFinishedHandler;
            }
        }

        public void OnUpdate(EnemyUnitController owner)
        {
        }

        public void OnFixedUpdate(EnemyUnitController owner)
        {
        }

        public void OnExit(EnemyUnitController owner)
        {
            if (onHitFinishedHandler != null)
            {
                owner.OnHitFinished -= onHitFinishedHandler;
            }

            onHitFinishedHandler = null;
        }
    }
}
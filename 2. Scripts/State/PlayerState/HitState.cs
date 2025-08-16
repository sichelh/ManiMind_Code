using System;
using UnityEngine;

namespace PlayerState
{
    public class HitState : IState<PlayerUnitController, PlayerUnitState>
    {
        private readonly int isHit = Define.HitAnimationHash;
        private bool canCounter;
        private Action onHitFinishedHandler;

        public void OnEnter(PlayerUnitController owner)
        {
            owner.IsAnimationDone = false;
            owner.Animator.SetTrigger(isHit);
            owner.PlayHitVoiceSound();
            Unit lastAttacker = owner.LastAttacker;
            canCounter = owner.CanCounterAttack(lastAttacker);
            if (lastAttacker == null)
            {
                return;
            }

            if (canCounter)
            {
                onHitFinishedHandler = () => owner.StartCounterAttack(lastAttacker);
            }
            else if (!lastAttacker.SkillController.CurrentSkillData?.skillSo.skillTimeLine)
            {
                onHitFinishedHandler = lastAttacker.InvokeHitFinished;
            }

            if (onHitFinishedHandler != null)
            {
                owner.OnHitFinished += onHitFinishedHandler;
            }
        }

        public void OnUpdate(PlayerUnitController owner)
        {
        }

        public void OnFixedUpdate(PlayerUnitController owner)
        {
        }

        public void OnExit(PlayerUnitController owner)
        {
            if (onHitFinishedHandler != null)
            {
                owner.OnHitFinished -= onHitFinishedHandler;
                onHitFinishedHandler = null;
            }
        }
    }
}
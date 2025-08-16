using UnityEngine;

namespace PlayerState
{
    public class SkillState : IState<PlayerUnitController, PlayerUnitState>
    {
        private readonly int skill = Define.SkillAnimationHash;
        private System.Action onTimelineEnd;

        public void OnEnter(PlayerUnitController owner)
        {
            owner.OnToggleNavmeshAgent(false);
            owner.IsAnimationDone = false;
            TimeLineManager.Instance.PlayTimeLine(CameraManager.Instance.cinemachineBrain, CameraManager.Instance.skillCameraController, owner, out bool isTimeLine);

            if (isTimeLine)
            {
                onTimelineEnd = () =>
                {
                    bool hasProjectile = owner.SkillController.IsCurrentSkillProjectile;
                    if (!hasProjectile)
                    {
                        owner.InvokeSkillFinished();
                    }

                    TimeLineManager.Instance.TimelineEnded -= onTimelineEnd;
                    onTimelineEnd = null;
                };
                TimeLineManager.Instance.TimelineEnded += onTimelineEnd;
            }

            else
            {
                owner.Animator.SetTrigger(Define.SkillAnimationHash);
                owner.SkillController.CurrentSkillData.skillSo.skillType.PlayCastVFX(owner, owner.Target);
                owner.SkillController.CurrentSkillData.skillSo.skillType.PlayCastSFX(owner);
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
            owner.Animator.ResetTrigger(skill);
            owner.OnToggleNavmeshAgent(true);

            if (onTimelineEnd != null)
            {
                TimeLineManager.Instance.TimelineEnded -= onTimelineEnd;
                onTimelineEnd = null;
            }
        }
    }
}
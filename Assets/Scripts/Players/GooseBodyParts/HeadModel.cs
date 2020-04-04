﻿using UnityEngine;
using System.Collections;
using DG.Tweening;

public class HeadModel : MonoBehaviour
{
    [SerializeField] private Head Head;

    public void OnPull()
    {
        Head.Goose.Body.BodyAnimator.SetFloat("Tail", 1.0f);
        Head.Goose.Body.BodyAnimator.SetFloat("Breath", 0.0f);
        AudioManager.Instance.SoundPlay("sfx/Sound_Pull");
        Head.Goose.Body.PullNeck();
        GoalBall ball = GameManager.Instance.Cur_BattleManager.Ball;
        Vector3 diff = GameManager.Instance.GetBallPosition() - transform.position;

        float distance = diff.magnitude;
        if (distance < Head.GooseConfig.PullRadius)
        {
            ball.RigidBody.DOMove(Head.transform.position + Head.transform.forward * Head.GooseConfig.PullBallStopFromHead, Head.GooseConfig.PullDuration);
            ball.Kick(Head.ParentPlayerControl.Player.PlayerInfo.TeamNumber, (-diff.normalized) * 0);
            FXManager.Instance.PlayFX(FX_Type.BallKickParticleSystem, GameManager.Instance.GetBallPosition(), Quaternion.FromToRotation(Vector3.back, diff.normalized));
        }
    }

    public void OnPush()
    {
        Head.Goose.Body.BodyAnimator.SetFloat("Tail", 1.0f);
        Head.Goose.Body.BodyAnimator.SetFloat("Breath", 0.0f);
        AudioManager.Instance.SoundPlay("sfx/Sound_Push");
        Head.Goose.Body.PushNeck();
        GoalBall ball = GameManager.Instance.Cur_BattleManager.Ball;
        Vector3 diff = GameManager.Instance.GetBallPosition() - transform.position;

        float distance = diff.magnitude;
        if (distance < Head.GooseConfig.PushRadius + Head.Goose.Body.ChargeDistance)
        {
            ball.Kick(Head.ParentPlayerControl.Player.PlayerInfo.TeamNumber, (Head.transform.forward) * (Head.GooseConfig.PushForce + Head.PushChargeForceRatio * Head.GooseConfig.PushChargingExtraForce));
            FXManager.Instance.PlayFX(FX_Type.BallKickParticleSystem, GameManager.Instance.GetBallPosition(), Quaternion.FromToRotation(Vector3.back, diff.normalized));
        }
    }

    public void OnPushOver()
    {
        Head.HeadStatus = Head.HeadStatusTypes.Idle;
        Head.HeadCollider.enabled = true;
        Head.Anim.ResetTrigger("Push");
        Head.Anim.ResetTrigger("Pull");
    }

    public void OnPullOver()
    {
        Head.HeadStatus = Head.HeadStatusTypes.Idle;
        Head.HeadCollider.enabled = true;
        Head.Anim.ResetTrigger("Push");
        Head.Anim.ResetTrigger("Pull");
    }
}
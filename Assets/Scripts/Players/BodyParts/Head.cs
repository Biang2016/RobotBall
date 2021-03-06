﻿using UnityEngine;

public class Head : MonoBehaviour
{
    internal Duck Duck;
    internal Player Player => Duck.Player;
    internal DuckConfig DuckConfig => Duck.DuckConfig;

    public void Attached()
    {
        Duck = GetComponentInParent<Duck>();
    }

    public void ExecuteCommand(bool leftTriggerDown, bool rightBumperDown, bool rightTriggerDown, bool rightTriggerUp, bool rightTriggerPressed)
    {
        switch (HeadStatus)
        {
            case HeadStatusTypes.Idle:
            {
                if (leftTriggerDown)
                {
                    Pull();
                }
                else if (rightTriggerDown)
                {
                    HeadStatus = HeadStatusTypes.PushCharging;
                    Duck.Ring.Charge();
                    Duck.Wings.Charge();
                    Duck.Feet.StartCharge();
                    Duck.SunGlasses.Charging();
                    PushChargeTimeTick = Time.time;
                    AudioDuck.Instance.StartPlayerChargeSound(Player.PlayerNumber, GameManager.Instance.Cur_BallBattleManager.BattleCamera.transform, Duck.DuckRigidbody);
                }
                else if (rightTriggerPressed)
                {
                    //HeadStatus = HeadStatusTypes.PushCharging;
                    //Duck.Feet.ShrinkChargingCircle();
                }
                else if (rightBumperDown)
                {
                    PushChargeForceRatio = 0f;
                    Push();
                    Duck.Feet.ReleaseChargingCircle();
                }

                break;
            }
            case HeadStatusTypes.PushCharging:
            {
                if (rightTriggerUp)
                {
                    PushChargeForceRatio = (Time.time - PushChargeTimeTick) / DuckConfig.PushChargeTimeMaxDuration;
                    PushChargeTimeTick = Time.time;
                    Push();
                    Duck.Feet.ReleaseChargingCircle();
                    Duck.SunGlasses.Normal();
                    AudioDuck.Instance.StopPlayerChargeSound(Player.PlayerNumber);
                }
                else if (rightTriggerPressed)
                {
                    if (Time.time - PushChargeTimeTick > DuckConfig.PushChargeTimeMaxDuration - 0.7f)
                    {
                        Duck.DuckUI.ShowMaxUI();
                    }

                    if (Time.time - PushChargeTimeTick > DuckConfig.PushChargeTimeMaxDuration)
                    {
                        PushChargeForceRatio = 1;
                        PushChargeTimeTick = Time.time;
                        Push();
                        Duck.Feet.ReleaseChargingCircle();
                        Duck.SunGlasses.Normal();
                        AudioDuck.Instance.StopPlayerChargeSound(Player.PlayerNumber);
                    }
                }

                break;
            }
        }
    }

    public void Initialize()
    {
        foreach (Collider c in transform.GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.layer == GameManager.Instance.Layer_BallKicker)
            {
                string layerName = "BallKicker" + ((int) (Player.PlayerNumber) + 1);
                int layer = LayerMask.NameToLayer(layerName);
                c.gameObject.layer = layer;
            }
        }
    }

    public Animator Anim;

    private float PushChargeTimeTick = 0;
    public HeadStatusTypes HeadStatus = HeadStatusTypes.Idle;

    public enum HeadStatusTypes
    {
        Idle = 0,
        PushCharging = 1,
        Pushing = 2,
        Pulling = 3,
    }

    internal float PushChargeForceRatio = 1.0f;

    private void Push()
    {
        HeadStatus = HeadStatusTypes.Pushing;
        Anim.SetTrigger("Push");
        Duck.Ring.Kick();
    }

    private void Pull()
    {
        HeadStatus = HeadStatusTypes.Pulling;
        string postfix = Random.Range(0, 2) == 1 ? "" : "2";
        Anim.SetTrigger("Pull" + postfix);
        Duck.Ring.Kick();
    }

    internal Vector3 Cur_HeadLookAtPosition;

    void LateUpdate()
    {
        if (GameManager.Instance.M_NetworkMode == GameManager.NetworkMode.Online)
        {
            if (!Player.entity.IsControllerOrOwner)
            {
                ExecuteCommand(
                    Player.state.Input.LeftTriggerDown,
                    Player.state.Input.RightBumperDown,
                    Player.state.Input.RightTriggerDown,
                    Player.state.Input.RightTriggerUp,
                    Player.state.Input.RightTrigger);
            }
        }

        transform.position = Duck.Neck.HeadPosPivot.position;
        if (!Duck.Body.IsPushingNeck)
        {
            Ball ball = GameManager.Instance.GetBallByHeadPos(transform.position);
            if (ball)
            {
                Vector3 diff_HeadToBody = Player.GetPlayerPosition - transform.position;
                Vector3 diff_HeadToBall = ball.transform.position - transform.position;

                float angle = Mathf.Abs(Vector3.SignedAngle(diff_HeadToBody, diff_HeadToBall, Vector3.down));

                if (angle > DuckConfig.LookBallAngleThreshold)
                {
                    transform.LookAt(ball.transform.position);
                }
                else
                {
                    transform.LookAt(transform.position - diff_HeadToBody.normalized * 10f);
                }

                HeadVerticalOffsetManual = 0f;
            }
            else
            {
                if (GameManager.Instance.M_NetworkMode == GameManager.NetworkMode.Local || Duck.Player.entity.HasControl)
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f || (Duck.Player.Controller != null && Duck.Player.Controller.ButtonPressed[ControlButtons.DPAD_Up]))
                    {
                        HeadVerticalOffsetManual += 0.5f;
                        HeadVerticalOffsetManual = Mathf.Clamp(HeadVerticalOffsetManual, HeadVerticalOffsetManualMin, HeadVerticalOffsetManualMax);
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f || (Duck.Player.Controller != null && Duck.Player.Controller.ButtonPressed[ControlButtons.DPAD_Down]))
                    {
                        HeadVerticalOffsetManual -= 0.5f;
                        HeadVerticalOffsetManual = Mathf.Clamp(HeadVerticalOffsetManual, HeadVerticalOffsetManualMin, HeadVerticalOffsetManualMax);
                    }

                    if (Duck.Player.Controller != null)
                    {
                        if (Duck.Player.Controller is KeyBoardController)
                        {
                            Ray ray = GameManager.Instance.Cur_BattleManager.BattleCamera.ScreenPointToRay(Input.mousePosition);
                            if (GameManager.Instance.Cur_BattleManager.FloorPlane.Raycast(ray, out float enter))
                            {
                                Vector3 mousePos = ray.GetPoint(enter);
                                mousePos.y = HeadVerticalOffsetManual + 2f;
                                Cur_HeadLookAtPosition = mousePos;
                                transform.LookAt(Cur_HeadLookAtPosition);
                            }
                        }
                        else if (Duck.Player.Controller is XBoxController)
                        {
                            Vector3 diff_HeadToBody = Player.GetPlayerPosition - transform.position;
                            Cur_HeadLookAtPosition = transform.position - diff_HeadToBody.normalized * 3f;
                            Cur_HeadLookAtPosition.y = HeadVerticalOffsetManual + 2f;
                            transform.LookAt(Cur_HeadLookAtPosition);
                        }
                    }
                }
                else
                {
                    transform.LookAt(Duck.Player.HeadLookAtPosition);
                }
            }
        }
    }

    private float HeadVerticalOffsetManual = 0f;
    [SerializeField] private float HeadVerticalOffsetManualMax = 8f;
    [SerializeField] private float HeadVerticalOffsetManualMin = -1f;
}
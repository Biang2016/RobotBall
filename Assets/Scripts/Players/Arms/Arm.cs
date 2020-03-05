﻿using System.Collections.Generic;
using UnityEngine;

public class Arm : PlayerUpperPart
{
    public RotateArmSection ArmRotate;
    public RotateArmSection Arm1;
    public RotateArmSection Arm2;
    public Transform StartPivot;
    public Transform ArmEndPivot;
    public ArmEnd ArmEnd;

    internal float SpeedModifier = 1f;

    void Awake()
    {
        ArmEnd = GetComponentInChildren<ArmEnd>();
    }

    protected override void Operate_Manual(PlayerNumber controllerIndex)
    {
        Vector3 tarPos = ArmEndPivot.position;

        tarPos += Vector3.forward * ParentPlayerControl.Player.ArmSpeed * SpeedModifier * MultiControllerManager.Instance.Controllers[controllerIndex].Axises[ControlAxis.RightStick_H];
        tarPos += Vector3.right * ParentPlayerControl.Player.ArmSpeed * SpeedModifier * MultiControllerManager.Instance.Controllers[controllerIndex].Axises[ControlAxis.RightStick_V];

        float targetRadius = (tarPos - transform.position).magnitude;
        if (targetRadius < ParentPlayerControl.Player.Radius * 2)
        {
            tarPos = (tarPos - transform.position).normalized * ParentPlayerControl.Player.Radius * 2 + transform.position;
        }

        MoveArmTo(tarPos);
    }

    protected override void Operate_AI()
    {
    }

    private void MoveArmTo(Vector3 targetPos)
    {
        Vector3 diff = Vector3.Scale(targetPos - StartPivot.position, new Vector3(1, 0, 1));
        float angleOffset = Vector3.SignedAngle(transform.forward, diff, Vector3.up);
        ArmRotate.SetRotation(angleOffset);

        float distance = Mathf.Clamp(diff.magnitude, Mathf.Abs(Arm1.Length - Arm2.Length) + 0.2f, Arm1.Length + Arm2.Length - 0.2f);

        float angle_Arm1 = -Mathf.Rad2Deg * Mathf.Acos((Arm1.Length * Arm1.Length + distance * distance - Arm2.Length * Arm2.Length) / (2 * Arm1.Length * distance));
        Arm1.SetRotation(angle_Arm1);

        float angle_Arm2 = 180 - Mathf.Rad2Deg * Mathf.Acos((Arm1.Length * Arm1.Length + Arm2.Length * Arm2.Length - distance * distance) / (2 * Arm1.Length * Arm2.Length));
        Arm2.SetRotation(angle_Arm2);
    }

    public List<float> GetAllSectionRotateAngle()
    {
        List<float> res = new List<float>();
        res.Add(ArmRotate.RotateAngle);
        res.Add(Arm1.RotateAngle);
        res.Add(Arm2.RotateAngle);
        return res;
    }

    public void ApplyRotateAngles(List<float> ras)
    {
        ArmRotate.SetRotation(ras[0]);
        Arm1.SetRotation(ras[1]);
        Arm2.SetRotation(ras[2]);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        ArmEnd.transform.position = ArmEndPivot.position;
    }
}
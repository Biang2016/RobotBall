﻿using UnityEngine;
using System.Collections;

public class FlagGetRingPoint : MonoBehaviour
{
    public TeamNumber MyTeamNumber;

    void OnTriggerEnter(Collider c)
    {
        if (BoltNetwork.IsServer)
        {
            if (c.gameObject.GetComponentInParent<PlayerCollider>())
            {
                Player p = c.GetComponentInParent<Player>();
                if (p.TeamNumber == MyTeamNumber)
                {
                    if (PlayerObjectRegistry.MyPlayer == p)
                    {
                        //Todo Vibrate
                    }

                    ((BattleManager_FlagRace) GameManager.Instance.Cur_BallBattleManager).GetFlagRing_Server(p);
                }
            }
        }
    }
}
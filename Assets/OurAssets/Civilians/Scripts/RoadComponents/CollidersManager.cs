using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersManager : MonoBehaviour
{

    [SerializeField]
    private List<InvisibleBlocker> blockers;


    public void EnableAllColliders(WaitingReason enableReason)
    {
        foreach (InvisibleBlocker blocker in blockers)
        {
            blocker.EnableCollider(enableReason);
        }
    }

    public void DisableAllColliders()
    {
        foreach (InvisibleBlocker blocker in blockers)
        {
            blocker.DisableCollider();
        }
    }

    public void EnableCollider(int index, WaitingReason enableReason)
    {
        if (index >= blockers.Count)
        {
            return;
        }
        blockers[index].EnableCollider(enableReason);
    }

    public void DisableCollider(int index)
    {
        if (index >= blockers.Count)
        {
            return;
        }
        blockers[index].DisableCollider();
    }
}

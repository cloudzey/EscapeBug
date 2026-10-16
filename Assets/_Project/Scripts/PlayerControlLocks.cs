
using System.Collections.Generic;
using UnityEngine;

public static class PlayerControlLocks
{
    private static readonly HashSet<Object> lockOwners = new();

    public static bool IsLocked => lockOwners.Count > 0;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetLocks()
    {
        lockOwners.Clear();
    }

    public static void Acquire(Object owner)
    {
        if (owner != null)
            lockOwners.Add(owner);
    }

    public static void Release(Object owner)
    {
        lockOwners.Remove(owner);
    }
}

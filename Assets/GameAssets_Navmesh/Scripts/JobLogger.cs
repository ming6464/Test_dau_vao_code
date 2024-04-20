using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public static class JobLogger
{
    [BurstDiscard] public static void Log(params object[] parts) => Debug.Log(AppendToString(parts));
    [BurstDiscard] public static void LogWarning(params object[] parts) => Debug.LogWarning(AppendToString(parts));
    [BurstDiscard] public static void LogError(params object[] parts) => Debug.LogError(AppendToString(parts));

    private static string AppendToString(params object[] parts)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0, len = parts.Length; i < len; i++) sb.Append(parts[i].ToString());
        return sb.ToString();
    }
}
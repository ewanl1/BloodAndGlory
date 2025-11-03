using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public static class ReparentPreserveWorld
{
    // Select: (1) all children you want to move, then (2) the TARGET parent last.
    // Run: Tools/Reparent Selected -> Last Selected (Preserve World)
    [MenuItem("Tools/Reparent Selected -> Last Selected (Preserve World)")]
    private static void ReparentToLastSelectedPreserveWorld()
    {
        var sel = Selection.transforms;
        if (sel == null || sel.Length < 2)
        {
            UnityEngine.Debug.LogError("Select one or more source objects FIRST, and the TARGET parent LAST.");
            return;
        }

        Transform target = sel[sel.Length - 1];
        if (PrefabUtility.IsPartOfPrefabAsset(target))
        {
            UnityEngine.Debug.LogError("Target must be a scene object, not a prefab asset.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(target.gameObject, "Reparent (Preserve World)");

        for (int i = 0; i < sel.Length - 1; i++)
        {
            Transform t = sel[i];

            if (t == target)
                continue;

            if (PrefabUtility.IsPartOfPrefabAsset(t))
            {
                UnityEngine.Debug.LogWarning($"Skipping prefab asset: {t.name}");
                continue;
            }

            // Cache world transform
            Vector3 wpos = t.position;
            Quaternion wrot = t.rotation;
            Vector3 wscale = t.lossyScale;

            Undo.SetTransformParent(t, target, "Reparent (Preserve World)");

            // SetParent(true) preserves world, but we force exactness to avoid any rounding or parent-lossyScale quirks.
            t.SetPositionAndRotation(wpos, wrot);
            // Recompute localScale exactly from desired world scale and new parent's world scale
            Vector3 pScale = target.lossyScale;
            t.localScale = new Vector3(
                SafeDivide(wscale.x, pScale.x),
                SafeDivide(wscale.y, pScale.y),
                SafeDivide(wscale.z, pScale.z)
            );
        }

        UnityEngine.Debug.Log($"Reparent complete. Children moved under '{target.name}' with world transforms preserved.");
    }

    private static float SafeDivide(float a, float b)
    {
        if (Mathf.Abs(b) < 1e-8f) return 0f;
        return a / b;
    }
}

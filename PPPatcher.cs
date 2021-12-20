using ColossalFramework.Math;
using HarmonyLib;
using MoveIt;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PropPainter {
    internal static class PPPatcher {
        private const string HARMONYID = @"com.quistar.PropPainter";

        private static IEnumerable<CodeInstruction> RenderInstanceTransplier(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getColor = AccessTools.Method(typeof(PropInfo), nameof(PropInfo.GetColor), new Type[] { typeof(Randomizer).MakeByRefType() });
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Callvirt && code.operand == getColor) {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PPManager), nameof(PPManager.GetPropColor)));
                } else {
                    yield return code;
                }
            }
        }

        private static void ActionAddPostfix(HashSet<Instance> selection) => PPManager.ActionAddHandler?.Invoke(selection);

        private static void ActionClonePostfix(Dictionary<Instance, Instance> ___m_origToCloneUpdate) => PPManager.ActionCloneHandler?.Invoke(___m_origToCloneUpdate);

        internal static void EnablePatch() {
            Harmony harmony = new Harmony(HARMONYID);
            harmony.Patch(AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance),
                new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int) }),
                transpiler: new HarmonyMethod(typeof(PPPatcher), nameof(RenderInstanceTransplier)));
        }

        internal static void DisablePatch() {
            Harmony harmony = new Harmony(HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance),
                new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int) }), HarmonyPatchType.Transpiler, HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(MyExtensions), nameof(MyExtensions.AddObject)), HarmonyPatchType.Postfix, HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(CloneActionBase), nameof(CloneActionBase.Do)), HarmonyPatchType.Postfix, HARMONYID);
        }

        internal static void AttachMoveItPostProcess() {
            Harmony harmony = new Harmony(HARMONYID);
            harmony.Patch(AccessTools.Method(typeof(MyExtensions), nameof(MyExtensions.AddObject)),
                postfix: new HarmonyMethod(typeof(PPPatcher), nameof(ActionAddPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CloneActionBase), nameof(CloneActionBase.Do)),
                postfix: new HarmonyMethod(typeof(PPPatcher), nameof(ActionClonePostfix)));
        }
    }
}

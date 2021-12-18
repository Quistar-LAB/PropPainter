using ColossalFramework.Math;
using HarmonyLib;
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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PPManager), nameof(PPManager.GetColor)));
                } else {
                    yield return code;
                }
            }
        }

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
        }
    }
}

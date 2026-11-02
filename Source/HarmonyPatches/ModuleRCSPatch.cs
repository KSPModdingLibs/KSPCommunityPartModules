using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Expansions.Missions.Adjusters;
using KSPCommunityPartModules.Modules;

namespace KSPCommunityPartModules.HarmonyPatches
{
    [HarmonyPatch(typeof(ModuleRCS), "FixedUpdate")]
    internal static class ModuleRCS_FixedUpdate_Patch
    {
        // only assigned by IL patch so ignore never assigned warning
#pragma warning disable CS0649
        public static int i;
        public static bool enableThrusters;

        // Allow individual control of each thruster using adjuster
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SetThrustLvl(ModuleRCS instance)
        {
            float thrustLevel = instance.currentThrustForce;
            int thrusterIndex = i;

            for (int i = 0; i < instance.adjusterCache.Count; i++)
            {
                AdjusterRCSBase adjuster = instance.adjusterCache[i];

                // Set thruster from adjuster
                if (adjuster is AdjusterIndividualRCS adjusterExt)
                {
                    thrustLevel = adjusterExt.ThrusterLevels[thrusterIndex];
                }
            }

            return thrustLevel;
        }

        // Check ThrusterLevels instead of just inputRot/inputLin being non-zero
        static void Prefix(ModuleRCS __instance)
        {
            if (__instance.adjusterCache.Count > 0)
            {
                bool hasExtAdjuster = false;
                for (int i = 0; i < __instance.adjusterCache.Count; i++)
                {
                    AdjusterRCSBase adjuster = __instance.adjusterCache[i];

                    if (adjuster is AdjusterIndividualRCS adjusterExt)
                    {
                        hasExtAdjuster = true;

                        // Only enable thruster not zero
                        if (!adjusterExt.ThrusterLevels.All(o => o == 0f))
                        {
                            enableThrusters = true;
                            return;
                        }
                    }
                }
                // All thrusters set to off by extended adjuster
                if (hasExtAdjuster)
                {
                    enableThrusters = false;
                    return;
                }
            }

            // Same as original code
            enableThrusters = (__instance.inputRot != Vector3.zero || __instance.inputLin != Vector3.zero);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var instructionsToInsert = new List<CodeInstruction>();
            var instructionsToInsert2 = new List<CodeInstruction>();
            var instructionsToInsert3 = new List<CodeInstruction>();

            // Jump to thrusters loop
            object lbThrusters = null;
            // Jump to DeactivateFX()
            object lbDeactivateFX = null;

            int compareStartIndex = -1;
            int compareEndIndex = -1;
            int loopIndex = -1;
            int calcIndex = -1;

            bool success = false;

            FieldInfo tcField = AccessTools.Field(typeof(ModuleRCS), nameof(ModuleRCS.tC));
            FieldInfo iRotField = AccessTools.Field(typeof(ModuleRCS), nameof(ModuleRCS.inputRot));
            MethodInfo calculateThrust = AccessTools.Method(typeof(ModuleRCS), nameof(ModuleRCS.CalculateThrust));

            FieldInfo iField = AccessTools.Field(typeof(ModuleRCS_FixedUpdate_Patch), nameof(i));
            FieldInfo enableField = AccessTools.Field(typeof(ModuleRCS_FixedUpdate_Patch), nameof(enableThrusters));
            MethodInfo setThrust = AccessTools.Method(typeof(ModuleRCS_FixedUpdate_Patch), nameof(SetThrustLvl));

            for (var i = 0; i < codes.Count; i++)
            {
                // Start of inputRot/inputLin check
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand == (object)iRotField)
                {
                    // Inject before ldarg.0 (not sure if ts matters)
                    compareStartIndex = i - 1;

                    // Find jump to thruster loop when non-zero input
                    for (; i < codes.Count; i++)
                    {
                        if (codes[i].opcode == OpCodes.Brtrue_S)
                        {
                            lbThrusters = codes[i].operand;
                            break;
                        }
                    }
                    if (lbThrusters == null)
                        break;

                    // Find the jump to DeactivateFX() when zero input
                    for (; i < codes.Count; i++)
                    {
                        if (codes[i].opcode == OpCodes.Brfalse)
                        {
                            lbDeactivateFX = codes[i].operand;

                            // There's 2 instructions after brfalse
                            compareEndIndex = i + 2;

                            i += 2;
                            break;
                        }
                    }
                    if (lbDeactivateFX == null)
                        break;

                    // Find CalculateThrust
                    for (; i < codes.Count; i++)
                    {
                        if (codes[i].opcode == OpCodes.Call && codes[i].operand == (object)calculateThrust)
                        {

                            // Modifies currentThrustForce value, despite name it's actually 0-1 thrust input
                            codes[i - 2] = new CodeInstruction(OpCodes.Call, setThrust);

                            calcIndex = i;
                            break;
                        }
                    }
                    if (calcIndex == -1)
                        break;

                    // Find "for (int i = 0; i < tC; i++)"
                    for (; i < codes.Count; i++)
                    {
                        if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand == (object)tcField &&
                            codes[i - 1].opcode == OpCodes.Ldarg_0)
                        {
                            // Inject before ldarg_0 pushes index
                            loopIndex = i - 1;
                            break;
                        }
                    }
                    if (loopIndex == -1)
                        break;

                    success = true;
                }
            }

            if (success)
            {
                // Store loop index
                instructionsToInsert3.Add(new CodeInstruction(OpCodes.Dup)); // stsfld pops from stack
                instructionsToInsert3.Add(new CodeInstruction(OpCodes.Stsfld, iField));
                codes.InsertRange(loopIndex, instructionsToInsert3);

                // Use value of enableThrusters instead of compare result
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldsfld, enableField));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, lbThrusters));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Br, lbDeactivateFX));

                // Delete the inputRot/inputLin code
                codes.RemoveRange(compareStartIndex, compareEndIndex - compareStartIndex - 1);
                codes.InsertRange(compareStartIndex, instructionsToInsert);
            }
            else
                Debug.LogError($"ModuleRCS patch failed");

            return codes.AsEnumerable();
        }
    }
}

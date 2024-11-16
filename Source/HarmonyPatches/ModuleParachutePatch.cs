using HarmonyLib;
using System;

namespace KSPCommunityPartModules.HarmonyPatches
{
    [HarmonyPatch(typeof(ModuleParachute))]
    public class ModuleParachuteEvents
    {
        public static void ModuleManagerPostLoad()
        {
            Harmony harmony = new Harmony("KSPCommunityPartModules.ModuleParachutePatch");
            harmony.PatchAll();
        }

        public static event Action<ModuleParachute> OnDeployed;
        public static event Action<ModuleParachute> OnRepacked;

        [HarmonyPostfix]
        [HarmonyPatch("OnParachuteSemiDeployed")]
        static void RaiseEventOnSemiDeployed(ModuleParachute __instance)
        {
            OnDeployed?.Invoke(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnParachuteFullyDeployed")]
        static void RaiseEventOnFullyDeployed(ModuleParachute __instance)
        {
            OnDeployed?.Invoke(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Repack")]
        static void RaiseEventOnRepack(ModuleParachute __instance)
        {
            OnRepacked?.Invoke(__instance);
        }
    }
}

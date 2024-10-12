/*
    Usecase:        Automatically cutting parachute modules that are specified as drogue chutes
    Example:        Bluedog Design Bureau Apollo command pod parachutes.
    Originally By:  Jsolson
    Originally For: Bluedog Design Bureau
*/
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    class ModuleAutoCutDrogue : PartModule
    {
        [KSPField]
        public bool isDrogueChute = false;

        [UI_Toggle(scene = UI_Scene.All, disabledText = "#autoLOC_439840", enabledText = "#autoLOC_439839")]
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#KSPCPM_CutDrogues")]
        public bool autoCutDrogue = true;

        [KSPField(isPersistant = true)]
        public bool triggered = false;

        private ModuleParachute chute = null;

        // Technically this creates an edge case where if two loaded vessels deploy a main parachute at the same time only one of the vessels will cut its drogue chutes,
        // but I feel like its unlikely enough to not warrent the effort required to fix it.
        private static int lastFrame;
        private bool IsChuteDeployed(ModuleParachute pParachute) => pParachute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED
                                                                 || pParachute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED;
        public override void OnStart(StartState state)
        {
            chute = part.FindModuleImplementing<ModuleParachute>();
            if (chute == null)
                Debug.LogError($"[{nameof(ModuleAutoCutDrogue)}] ModuleParachute not found on part {part.partInfo.title}");

            Fields[nameof(autoCutDrogue)].guiActive = !isDrogueChute;
            Fields[nameof(autoCutDrogue)].guiActiveEditor = !isDrogueChute;

            if (!isDrogueChute) ModuleParachuteEvents.OnDeployed += OnParachuteDeployed; 
            if (!isDrogueChute) ModuleParachuteEvents.OnRepacked += OnParachuteRepacked;
        }

        public void OnDestroy()
        {
            ModuleParachuteEvents.OnDeployed -= OnParachuteDeployed;
            ModuleParachuteEvents.OnRepacked -= OnParachuteRepacked;
        }

        private void OnParachuteDeployed(ModuleParachute pChute)
        {
            if (autoCutDrogue && !triggered && pChute == chute)
            {
                if (lastFrame != Time.frameCount)
                {
                    var drogues = vessel.FindPartModulesImplementing<ModuleAutoCutDrogue>().Where(d => d.isDrogueChute && d.chute != null);
                    foreach (ModuleAutoCutDrogue d in drogues)
                    {
                        if (IsChuteDeployed(d.chute)) d.chute.CutParachute();
                    }
                    lastFrame = Time.frameCount;
                }
                triggered = true;
            }
        }

        private void OnParachuteRepacked(ModuleParachute pChute)
        {
            if (triggered && pChute == chute) triggered = false;
        }
    }

    [HarmonyPatch(typeof(ModuleParachute))]
    internal class ModuleParachuteEvents
    {
        public static void ModuleManagerPostLoad()
        {
            Harmony harmony = new Harmony("KSPCommunityPartModules");
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

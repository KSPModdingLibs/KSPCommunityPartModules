/*
    Usecase:        Automatically cutting parachute modules that are specified as drogue chutes
    Example:        Bluedog Design Bureau Apollo command pod parachutes.
    Originally By:  Jsolson
    Originally For: Bluedog Design Bureau
*/
using System.Linq;
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    class ModuleAutoCutDrogue : PartModule
    {
        [KSPField]
        public bool isDrogueChute = false;

        [UI_Toggle(scene = UI_Scene.All, disabledText = "No", enabledText = "Yes")]
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#KSPCPM_CutDrogues")]
        public bool autoCutDrogue = true;

        [KSPField(isPersistant = true)]
        public bool triggered = false;

        private ModuleParachute chute = null;
        private bool IsChuteDeployed(ModuleParachute pParachute) => pParachute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED
                                                                 || pParachute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED;
        public override void OnStart(StartState state)
        {
            chute = part.FindModulesImplementing<ModuleParachute>().FirstOrDefault();
            if (chute == null)
                Debug.LogError($"[{nameof(ModuleAutoCutDrogue)}] ModuleParachute not found on part {part.partInfo.title}");

            Fields[nameof(autoCutDrogue)].guiActive = !isDrogueChute;
            Fields[nameof(autoCutDrogue)].guiActiveEditor = !isDrogueChute;
        }

        public void FixedUpdate()
        {
            if (isDrogueChute || chute == null)
            {
                this.isEnabled = false;
                this.enabled = false;
                return;
            }

            if (IsChuteDeployed(chute))
            {
                if (!triggered)
                {
                    var drogues = vessel.FindPartModulesImplementing<ModuleAutoCutDrogue>().Where(d => d.isDrogueChute && d.chute != null);
                    foreach (ModuleAutoCutDrogue d in drogues)
                    {
                        if (IsChuteDeployed(d.chute)) d.chute.CutParachute();
                    }
                    triggered = true;
                }
            }
            else if (chute.deploymentState == ModuleParachute.deploymentStates.STOWED)
            {
                triggered = false;
            }
        }
    }
}

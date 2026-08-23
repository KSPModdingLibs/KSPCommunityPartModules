/*
    Usecase:        Making a part wait for it's engine to burn out before decoupling
    Example:        BDB spin motors and DAXA's Epsilon spin table
    Originally By:  jsolson
    Originally For: BluedogDesignBureau
*/
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KSPCommunityPartModules
{
    class ModuleDecoupleAfterBurn : ModuleDecouple
    {
        [KSPField(guiActive = true, isPersistant = true, guiActiveEditor = true, guiName = "Auto Jettison"), UI_Toggle()]
        public bool autoDecouple = true;

        private ModuleEngines engine;
        private bool wasRunning = false;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            engine = part.FindModulesImplementing<ModuleEngines>().FirstOrDefault();
        }

        public override void OnActive()
        {
            //base.OnActive();
            staged = true;
        }

        public override void OnUpdate()
        {
            if (isDecoupled)
                return;
            if (engine == null)
                return;
            if (autoDecouple)
            {
                if (!wasRunning)
                {
                    wasRunning = engine.GetCurrentThrust() > 0;
                }
                else
                {
                    if (engine.GetCurrentThrust() <= 0)
                        Decouple();
                }
            }
        }
    }

    class ModuleAnchoredDecoupleAfterBurn : ModuleAnchoredDecoupler
    {
        [KSPField(guiActive = true, isPersistant = true, guiActiveEditor = true, guiName = "Auto Jettison"), UI_Toggle()]
        public bool autoDecouple = true;

        private ModuleEngines engine;
        private bool wasRunning = false;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            engine = part.FindModulesImplementing<ModuleEngines>().FirstOrDefault();
        }

        public override void OnActive()
        {
            //base.OnActive();
            staged = true;
        }

        public override void OnUpdate()
        {
            if (isDecoupled)
                return;
            if (engine == null)
                return;
            if (autoDecouple)
            {
                if (!wasRunning)
                {
                    wasRunning = engine.GetCurrentThrust() > 0;
                }
                else
                {
                    if (engine.GetCurrentThrust() <= 0)
                        Decouple();
                }
            }
        }
    }
}

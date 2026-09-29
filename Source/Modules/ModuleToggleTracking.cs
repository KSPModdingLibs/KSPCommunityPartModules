/*
    Usecase:        This module is applied to parts such as extendable solar panels and radiators to toggle their ability to track the sun.
    Originally By:  Spartwo
    Originally For: Kerbal Powers
    License:        GNU General Public License v3.0, see https://www.gnu.org/licenses/gpl-3.0.html
*/

using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleToggleTracking : PartModule
    {

        [KSPEvent(guiActive = true,
            guiActiveEditor = true,
            guiName = "#KSPCPM_Tracking")]
        public void EventToggleTracking() => SetTracking(!trackingEnabled);


        [KSPAction("#KSPCPM_ToggleTracking")]
        public void AGToggleTracking(KSPActionParam param) => SetTracking(!trackingEnabled);

        [KSPAction("#KSPCPM_DisableTracking")]
        public void AGDisableTracking(KSPActionParam param) => SetTracking(false);

        [KSPAction("#KSPCPM_EnableTracking")]
        public void AGEnableTracking(KSPActionParam param) => SetTracking(true);

        [KSPField(isPersistant = true)]
        public bool trackingEnabled;

        List<ModuleDeployablePart> trackers;
        private BaseEvent toggleEvent;

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);
            toggleEvent = Events["EventToggleTracking"];
            Apply(state);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Apply(state);
        }

        public void Apply(StartState state)
        {
            Debug.Log("tracking enabled" + trackingEnabled);

            try
            {
                // Find Tracker modules
                if (trackers == null)
                    trackers = part.FindModulesImplementing<ModuleDeployablePart>();

                // If there's still null that's a problem
                if (trackers == null)
                {
                    Debug.LogWarning($"[ModuleToggleTracking] No ModuleDeployablePart on '{part.name}'");
                    return;
                }

                if (state != StartState.Editor)
                {
                    SetTracking(trackingEnabled);
                }
                else
                {
                    UpdateToggleName();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[ModuleToggleTracking] {e}");
            }
        }

        private void SetTracking(bool newState)
        {
            trackingEnabled = newState;
            foreach (var tracker in trackers) tracker.isTracking = newState;
            UpdateToggleName();
        }


        private void UpdateToggleName()
        {
            if (toggleEvent == null) return;
            toggleEvent.guiName = Localizer.Format(trackingEnabled
                ? "#KSPCPM_DisableTracking"
                : "#KSPCPM_EnableTracking");
        }
    }
}

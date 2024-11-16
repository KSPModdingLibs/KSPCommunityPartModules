/*
    Usecase:        Making the Center of Pressure, Mass or Lift follow a transform on the same part.
    Example:        BoringCrewServices Starliner main parachutes,
                    StarshipExpansionProject Starship Flaps
    Originally By:  Sofie Brink & JonnyOThan
    Originally For: KSPCommunityPartModules
*/
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleCenterFollowTransform : PartModule
    {
        public const string MODULENAME = nameof(ModuleCenterFollowTransform);

        [KSPField]
        public bool enableCoP = false;

        [KSPField]
        public bool enableCoM = false;

        [KSPField]
        public bool enableCoL = false;

        [KSPField]
        public string transformName;

        [SerializeField]
        private Transform followTransform;

        // TODO: Reset offset to prefab value if B9PS disables one of the fields.
        public override void OnLoad(ConfigNode node)
        {
            bool anyModeActive = enableCoP || enableCoM || enableCoL;

            if (followTransform == null || followTransform.name != transformName)
            {
                if (transformName != null) followTransform = part.FindModelTransform(transformName);
                if (followTransform == null) Debug.LogError($"[{MODULENAME}] transformName '{transformName}' was empty or does not exist on part '{part.partInfo?.name}'");
            }
            if (!anyModeActive)
            {
                Debug.LogWarning($"[{MODULENAME}] no center is following transformName '{transformName}' on part '{part.partInfo?.name}'");
            }

            if (followTransform == null && part.partInfo != null)
            {
                // this may be important if someone is swapping out versions of this module with B9PS
                // Note this probably isn't correct for parts that also have modules that mess with this field (e.g. ModuleProceduralFairing)
                if (enableCoP) part.CoPOffset = part.partInfo.partPrefab.CoPOffset;
                if (enableCoM) part.CoMOffset = part.partInfo.partPrefab.CoMOffset;
                if (enableCoL) part.CoLOffset = part.partInfo.partPrefab.CoLOffset;
            }
            
            // NOTE: isEnabled will be persisted to the save file, but we want to treat it purely as runtime state
            isEnabled = followTransform != null && anyModeActive;
            enabled = followTransform != null && anyModeActive && HighLogic.LoadedSceneIsFlight;
        }

        public void FixedUpdate()
        {
            // Note that we shouldn't ever get here if the transform just didn't exist
            // But it's certainly possible for *something* to delete it later
            if (followTransform == null)
            {
                isEnabled = false;
                enabled = false;
            }
            else
            {
                Vector3 offset = part.transform.InverseTransformPoint(followTransform.position);
                if (enableCoP) part.CoPOffset = offset;
                if (enableCoM) part.CoMOffset = offset;
                if (enableCoL) part.CoLOffset = offset;
            }
        }
    }
}

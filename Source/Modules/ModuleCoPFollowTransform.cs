/*
    Usecase:        Making the Centre of Pressure follow a transform on the same part.
    Example:        BoringCrewServices Starliner main parachutes
    Originally By:  Sofie Brink
    Originally For: KSPCommunityPartModules
*/
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleCoPFollowTransform : PartModule
    {
        public const string MODULENAME = nameof(ModuleCoPFollowTransform);

        [KSPField]
        public string transformName;

        [SerializeField]
        private Transform followTransform;

        public override void OnLoad(ConfigNode node)
        {
            if (followTransform == null || followTransform.name != transformName)
            {
                if (transformName != null) followTransform = part.FindModelTransform(transformName);
                if (followTransform == null) Debug.LogError($"[{MODULENAME}] transformName '{transformName}' was empty or does not exist on part '{part.partInfo?.name}'");
            }

            if (followTransform == null)
            {
                // this may be important if someone is swapping out versions of this module with B9PS
                part.CoPOffset = Vector3.zero;
            }

            // NOTE: isEnabled will be persisted to the save file, but we want to treat it purely as runtime state
            this.isEnabled = followTransform != null;
            this.enabled = followTransform != null && HighLogic.LoadedSceneIsFlight;
        }

        public void FixedUpdate()
        {
            // Note that we shouldn't ever get here if the transform just didn't exist
            // But it's certainly possible for *something* to delete it later
            if (followTransform == null)
            {
                isEnabled = false;
                enabled = false;
                // should we reset CoPOffset here?   Not sure.
            }
            else    
            {
                part.CoPOffset = part.transform.InverseTransformPoint(followTransform.position);
            }
        }
    }
}

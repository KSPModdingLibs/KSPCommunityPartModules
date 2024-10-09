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
                if (followTransform == null) Debug.LogError($"[{MODULENAME}] transformName was empty or does not exist.");
                this.isEnabled = followTransform != null;
                this.enabled = followTransform != null;
            }
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            if (followTransform != null) part.CoPOffset = part.transform.InverseTransformPoint(followTransform.position);
        }
    }
}

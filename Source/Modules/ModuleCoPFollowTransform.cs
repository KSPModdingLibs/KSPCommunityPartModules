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

        private Transform followTransform;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (HighLogic.LoadedScene != GameScenes.LOADING)
            {
                if (transformName != null) followTransform = part.FindModelTransform(transformName);
                if (followTransform == null) Debug.LogError($"[{MODULENAME}] transformName was empty or does not exist.");
            }
        }

        public void FixedUpdate()
        {
            if (followTransform != null) part.CoPOffset = part.transform.InverseTransformPoint(followTransform.position);
        }
    }
}

/*
    Usecase:        Part Gameobject Visibility based on Stack Attachment Node Occupancy with support for multiple nodes per part. 
    Originally By:  Spartwo
    Originally For: Kerbal Powers
    License:        GNU General Public License v3.0, see https://www.gnu.org/licenses/gpl-3.0.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleAttachmentVisuals : PartModule
    {
        [KSPField]
        public string requiredNodes;

        //visible transforms when node occupied
        [KSPField]
        public string showAttached;

        //visible transforms when node unoccupied
        [KSPField]
        public string showFree;

        //"<objectDisplayName> Enabled/Disabled" in the editor
        [KSPField]
        public string objectDisplayName;

        [KSPEvent(
            guiActive = false,
            guiActiveEditor = true,
            guiName = "#KSPCPM_AttachmentVisual"
        )]
        public void EventToggleVisual() => ToggleVisual();

        [KSPField(isPersistant = true)]
        public bool transformEnabled = true;

        // Nodes used as conditions
        private List<AttachNode> nodes = new List<AttachNode>();

        // Transforms shown when nodes are occupied
        private List<Transform> attachedTransforms = new List<Transform>();

        // Transforms shown when nodes are free
        private List<Transform> freeTransforms = new List<Transform>();

        private HashSet<Part> directChildren = new HashSet<Part>();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(OnEditorEvent);
            }

            CacheInitialChildren();
            ParseConfig();
            UpdateVisuals();

            // make this module cheaper in update loops
            enabled = false;
        }

        public void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Remove(OnEditorEvent);
            }
        }

        private void ParseConfig()
        {
            nodes.Clear();
            attachedTransforms.Clear();
            freeTransforms.Clear();

            // Parse attachment nodes
            if (!string.IsNullOrWhiteSpace(requiredNodes))
            {
                foreach (string nodeName in requiredNodes.Split(';'))
                {
                    string nodeId = nodeName.Trim();

                    if (string.IsNullOrEmpty(nodeId))
                        continue;

                    AttachNode node = part.FindAttachNode(nodeId);

                    if (node != null)
                    {
                        nodes.Add(node);

                        Debug.Log(
                            $"[ModuleAttachmentVisuals] Found node '{nodeId}' " +
                            $"for part '{part.name}'"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[ModuleAttachmentVisuals] Node '{nodeId}' " +
                            $"not found on part '{part.name}'"
                        );
                    }
                }
            }

            // Parse transforms shown when condition is true
            if (!string.IsNullOrWhiteSpace(showAttached))
            {
                foreach (string transformName in showAttached.Split(';'))
                {
                    string name = transformName.Trim();

                    if (string.IsNullOrEmpty(name))
                        continue;

                    Transform transform = part.FindModelTransform(name);

                    if (transform != null)
                    {
                        attachedTransforms.Add(transform);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[ModuleAttachmentVisuals] Could not find attached transform " +
                            $"'{name}' on '{part.name}'"
                        );
                    }
                }
            }

            // Parse transforms shown when condition is false
            if (!string.IsNullOrWhiteSpace(showFree))
            {
                foreach (string transformName in showFree.Split(';'))
                {
                    string name = transformName.Trim();

                    if (string.IsNullOrEmpty(name))
                        continue;

                    Transform transform = part.FindModelTransform(name);

                    if (transform != null)
                    {
                        freeTransforms.Add(transform);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[ModuleAttachmentVisuals] Could not find free transform " +
                            $"'{name}' on '{part.name}'"
                        );
                    }
                }
            }
        }

        private void UpdateVisuals()
        {
            // The objects only show when:
            // - transform is enabled
            // - all config nodes are occupied

            bool allNodesAttached = nodes.Count > 0;
            for (int i = 0; i < nodes.Count && allNodesAttached; i++)
                allNodesAttached = nodes[i].attachedPart != null;

            bool visualActive = transformEnabled && allNodesAttached;

            SetTransforms(attachedTransforms, visualActive);
            SetTransforms(freeTransforms, !visualActive);

            UpdateToggleEventUI(allNodesAttached);
        }

        private void UpdateToggleEventUI(bool allNodesAttached)
        {
            BaseEvent toggleEvent = Events["EventToggleVisual"];

            // Only offer the toggle when there's actually something capped to toggle
            toggleEvent.active = allNodesAttached;

            string adjective = transformEnabled 
                ? Localizer.Format("#autoLOC_900889") 
                : Localizer.Format("#autoLOC_247995");

            string displayName = string.IsNullOrWhiteSpace(objectDisplayName)
                ? Localizer.Format("#KSPCPM_AttachmentVisual")
                : Localizer.Format(objectDisplayName);

            toggleEvent.guiName = $"{displayName} {adjective}";
        }

        private void SetTransforms(List<Transform> transforms, bool active)
        {
            foreach (Transform transform in transforms)
            {
                if (transform == null)
                {
                    Debug.LogWarning(
                        $"[ModuleAttachmentVisuals] Transform is null on part '{part.name}'"
                    );

                    continue;
                }

                transform.gameObject.SetActive(active);
            }
        }

        private void ToggleVisual()
        {
            transformEnabled = !transformEnabled;

            UpdateVisuals();
        }

        private void OnEditorEvent(ConstructionEventType evt, Part p)
        {
            if (
                evt != ConstructionEventType.PartAttached &&
                evt != ConstructionEventType.PartDetached
            )
            {
                return;
            }

            // Event directly involving this part
            if (part == p)
            {
                CacheInitialChildren();
                UpdateVisuals();
                return;
            }

            bool wasDirectChild = directChildren.Contains(p);
            bool isDirectChildNow = p.parent == part;

            switch (evt)
            {
                case ConstructionEventType.PartAttached:

                    if (isDirectChildNow)
                    {
                        directChildren.Add(p);
                        UpdateVisuals();
                    }

                    break;

                case ConstructionEventType.PartDetached:

                    if (wasDirectChild)
                    {
                        directChildren.Remove(p);
                        UpdateVisuals();
                    }

                    break;
            }
        }

        private void CacheInitialChildren()
        {
            directChildren.Clear();

            foreach (Part child in part.children)
            {
                directChildren.Add(child);
            }
        }
    }
}

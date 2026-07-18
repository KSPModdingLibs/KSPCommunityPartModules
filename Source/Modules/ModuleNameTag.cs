/*
    Usecase:        A user-editable name tag on any part, readable and writable by other mods.
    Example:        kOS's part:TAG suffix and kRPC's Part.Tag both read and write this module's value.
    Originally By:  kOS contributors (Dunbaratu et al.)
    Originally For: kOS
    License:        GNU General Public License v3.0, see https://www.gnu.org/licenses/gpl-3.0.html
*/
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleNameTag : PartModule
    {
        public const string MODULENAME = nameof(ModuleNameTag);

        private const string PAWGroup = "Name Tag";

        private NameTagWindow typingWindow;

        [KSPField(isPersistant = true,
                  guiActive = true,
                  guiActiveEditor = true,
                  guiName = "name tag",
                  groupName = PAWGroup,
                  groupDisplayName = PAWGroup)]
        public string nameTag = "";

        [KSPEvent(guiActive = true,
                  guiActiveEditor = true,
                  guiName = "Change Name Tag",
                  groupName = PAWGroup,
                  groupDisplayName = PAWGroup)]
        public void PopupNameTagChanger()
        {
            if (typingWindow != null)
                typingWindow.Close();
            if (HighLogic.LoadedSceneIsEditor)
            {
                EditorFacility whichEditor = EditorLogic.fetch.ship.shipFacility;
                if (!CanTagInEditor(whichEditor))
                {
                    var formattedString = string.Format("The {0} requires an upgrade to assign name tags", whichEditor);
                    ScreenMessages.PostScreenMessage(formattedString, 6, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }
            // Make a new instance of typingWindow, replacing the existing one if there was one:
            NameTagWindow oldTypingWindow = gameObject.GetComponent<NameTagWindow>();
            if (oldTypingWindow != null)
                Destroy(oldTypingWindow);
            typingWindow = gameObject.AddComponent<NameTagWindow>();
            typingWindow.Invoke(this, nameTag);
        }

        // For kOS issue #2764, this enforces a rule that says regardless of what ModuleManager
        // rules end up doing, there shall only ever be one name tag module per part:
        public override void OnAwake()
        {
            // If other instances of me exist in this part, remove them.  I am replacing them:
            for (int i = part.Modules.Count - 1; i >= 0; --i)
            {
                PartModule pm = part.Modules[i];
                if (pm != this && pm is ModuleNameTag)
                {
                    Debug.Log(string.Format(
                        "[{0}] Removing duplicate name tag PartModule from {1}.  Only one tag per part is supported.",
                        MODULENAME, part.name));
                    part.RemoveModule(pm);
                }
            }
        }

        public void TypingDone(string newValue)
        {
            nameTag = newValue;
            TypingCancel();
        }

        public void TypingCancel()
        {
            typingWindow.Close();
            Destroy(typingWindow);
            typingWindow = null;
        }

        /// <summary>
        /// Whether the current editor building is upgraded enough to allow assigning a name tag.
        /// In flight there is no such restriction. Tagging unlocks at the same point the game starts
        /// unlocking basic action groups.
        /// </summary>
        private static bool CanTagInEditor(EditorFacility whichEditor)
        {
            float buildingLevel;
            switch (whichEditor)
            {
                case EditorFacility.VAB:
                    buildingLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                    break;
                case EditorFacility.SPH:
                    buildingLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.SpaceplaneHangar);
                    break;
                default:
                    return false;
            }
            return GameVariables.Instance.UnlockedActionGroupsStock(buildingLevel, false);
        }
    }
}

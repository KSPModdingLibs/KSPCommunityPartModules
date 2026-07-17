using HarmonyLib;
using KSPCommunityPartModules.Modules;

namespace KSPCommunityPartModules.HarmonyPatches
{
    /// <summary>
    /// Migrates legacy name-tag save data onto the renamed <see cref="ModuleNameTag"/>.
    ///
    /// kOS and kRPC each used to ship a part module named "KOSNameTag"; that module now lives here as
    /// ModuleNameTag. Craft and saves made with the old mods store their tag in a
    /// MODULE { name = KOSNameTag, nameTag = "..." } node. KSP binds a saved MODULE node to a prefab
    /// module by matching the node's "name", so without this patch an old node would not match the
    /// ModuleNameTag prefab module and the tag would be lost. Both the editor craft path
    /// (ShipConstruct.LoadShip) and the flight/persistence path (ProtoPartModuleSnapshot.Load) funnel
    /// through Part.LoadModule, so rewriting the node's module name there covers both. The nameTag
    /// value key is unchanged, so it binds directly once the module name matches.
    ///
    /// This patch is applied by the assembly-wide Harmony.PatchAll() bootstrap in
    /// ModuleParachuteEvents.ModuleManagerPostLoad; it deliberately has no bootstrap of its own so the
    /// parachute patches are not applied twice.
    /// </summary>
    [HarmonyPatch(typeof(Part), nameof(Part.LoadModule))]
    public static class NameTagMigrationPatch
    {
        private const string LegacyModuleName = "KOSNameTag";

        [HarmonyPrefix]
        static void Prefix(ConfigNode node)
        {
            if (node == null || node.GetValue("name") != LegacyModuleName)
                return;

            // If a real KOSNameTag PartModule type is still loaded (an old kOS/kRPC, or the retired
            // standalone NameTag mod), leave the node alone so we don't misroute it to ModuleNameTag.
            if (AssemblyLoader.GetClassByName(typeof(PartModule), LegacyModuleName) != null)
                return;

            node.SetValue("name", ModuleNameTag.MODULENAME);
        }
    }
}

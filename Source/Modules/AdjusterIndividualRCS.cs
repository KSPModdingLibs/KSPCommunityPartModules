using Expansions.Missions;
using Expansions.Missions.Adjusters;

namespace KSPCommunityPartModules.Modules
{
    ///<Summary>
    /// Use this adjuster with ModuleRCS to individually control each thruster.
    ///</Summary>
    public class AdjusterIndividualRCS : AdjusterRCSBase
    {
        ///<Summary>
        /// Constructor
        ///</Summary>
        public AdjusterIndividualRCS() { }

        ///<Summary>
        /// Constructor
        ///</Summary>
        public AdjusterIndividualRCS(MENode node) : base(node) { }

        ///<Summary>
        /// Thrust level per thruster, normalized 0-1 value. Must be initialized to size of <see cref="ModuleRCS.thrusterTransforms"/>.
        ///</Summary>
        public float[] ThrusterLevels { get; set; }
    }
}

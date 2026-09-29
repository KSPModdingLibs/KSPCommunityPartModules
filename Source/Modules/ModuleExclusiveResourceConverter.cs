/*
    Usecase:        Extends the stock resource converter so that only one type can run at a time.
    Originally By:  Spartwo
    Originally For: Kerbal Powers
    License:        GNU General Public License v3.0, see https://www.gnu.org/licenses/gpl-3.0.html
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPCommunityPartModules.Modules
{
    public class ModuleExclusiveResourceConverter : ModuleResourceConverter
    {
        public override void StartResourceConverter()
        {
            StopOtherConverters();
            base.StartResourceConverter();
        }

        private void StopOtherConverters ()
        {
            ModuleExclusiveResourceConverter[] otherConverters = part.GetComponents<ModuleExclusiveResourceConverter>();
            foreach (ModuleExclusiveResourceConverter e in otherConverters) 
            {
                //Guard against stopping itself or already stopped converters
                if (e != this && e.IsActivated) e.StopResourceConverter();
            }
        }

    }
}

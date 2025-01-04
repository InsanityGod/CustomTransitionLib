using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(InventoryBase), "GetDefaultTransitionSpeedMul")]
    public static class AppendGetDefaultTransitionSpeedMultiplier
    {
        public static bool Prefix(EnumTransitionType transitionType, ref float __result)
        {
            var handler = transitionType.GetCustomHandler();
            if (handler == null) return true;

            __result = handler.GetDefaultTransitionSpeedMul(transitionType);
            return false;
        }
    }
}

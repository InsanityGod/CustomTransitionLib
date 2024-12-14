using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetTransitionRateMul))]
    public static class AppendGetTransitionSpeedMultiplier
    {
        public static void Postfix(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, ref float __result)
        {
            var handler = transType.GetCustomHandler();
            if (handler == null) return;

            __result = handler.GetTransitionRateMul(world, inSlot, transType, __result);
        }
    }
}

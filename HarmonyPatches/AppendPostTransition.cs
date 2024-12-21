using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.HarmonyPatches
{

    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.OnTransitionNow))]
    public static class AppendPostTransition
    {
        public static void Postfix(CollectibleObject __instance, ItemSlot slot, TransitionableProperties props, ref ItemStack __result)
        {
            var handler = props.Type.GetCustomHandler();
            if (handler == null) return;

            handler.PostOnTransitionNow(__instance, slot, props, ref __result);
        }
    }
}

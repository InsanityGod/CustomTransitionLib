using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleObject), "AppendPerishableInfoText", argumentTypes: new Type[] { typeof(ItemSlot), typeof(StringBuilder), typeof(IWorldAccessor), typeof(TransitionState) , typeof(bool) })]
    public static class AppendAppendPerishableInfoText
    {
        public static bool Prefix(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
        {
            var handler = state.Props.Type.GetCustomHandler();
            if (handler == null) return true;

            handler.AppendAppendPerishableInfoText(inSlot, dsc, world, state, nowSpoiling);

            return false;
        }
    }
}

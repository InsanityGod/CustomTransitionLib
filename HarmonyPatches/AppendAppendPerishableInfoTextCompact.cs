using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(BlockLiquidContainerBase), nameof(BlockLiquidContainerBase.PerishableInfoCompact))]
    public static class AppendAppendPerishableInfoTextCompact
    {
        //TODO compact container method? (from block entity shelf)
        public static void Postfix(ICoreAPI Api, ItemSlot contentSlot, ref string __result)
        {
            TransitionState[] transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);
            if(transitionStates == null) return;
            var builder = new StringBuilder(__result);
            builder.AppendLine();
            
            foreach(var transitionState in transitionStates) 
            {
                var handler = transitionState.Props.Type.GetCustomHandler();
                if(handler == null) continue;

                //TODO figure out what to do with nowSpoiling
                //TODO maybe a seperate method for compact info
                handler.AppendAppendPerishableInfoText(contentSlot, builder, Api.World, transitionState, false);
            }

            __result = builder.ToString();
        }
    }
}

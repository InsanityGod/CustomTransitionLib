using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace CustomTransitionLib.HarmonyPatches.FixingVanillaCode
{
    [HarmonyPatch]
    public static class FixFreshness
    {
        [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.CarryOverFreshness), new Type[] { typeof(ICoreAPI), typeof(ItemSlot[]), typeof(ItemStack[]), typeof(TransitionableProperties)})]
        [HarmonyPrefix]
        public static bool ReplaceCarryOverFreshness(CollectibleObject __instance, ICoreAPI api, ItemSlot[] inputSlots, ItemStack[] outStacks, TransitionableProperties perishProps)
        {
            float transitionedHoursRelative = 0f;
			float spoilageRelMax = 0f;
			float spoilageRel = 0f;
			int quantity = 0;

			foreach (ItemSlot slot in inputSlots)
			{
                TransitionState state = slot.Itemstack?.Collectible?.UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish);
                if(state == null) continue;

				quantity++;

				float spoilageRelOne = Math.Max(0f, (state.TransitionedHours - state.FreshHours) / state.TransitionHours);
				spoilageRelMax = Math.Max(spoilageRelOne, spoilageRelMax);
				spoilageRel += spoilageRelOne;
                transitionedHoursRelative += state.TransitionedHours / (state.TransitionHours + state.FreshHours);
			}

			transitionedHoursRelative /= (float)Math.Max(1, quantity);
			spoilageRel /= (float)Math.Max(1, quantity);

            var dummySlot = new DummySlot();
            foreach (var outStack in outStacks)
            {
                if(outStack == null) continue;

                dummySlot.Itemstack = outStack;
                var transitions = outStack.Collectible?.GetTransitionableProperties(api.World, outStack, null);
                if(transitions == null) continue;
                var states = outStack?.Collectible?.UpdateAndGetTransitionStates(api.World, dummySlot);
                if(states == null) continue;

                for (int transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
                {
                    if (transitions[transitionIndex].Type != EnumTransitionType.Perish) continue;
                    if (outStack.Attributes["transitionstate"] is not ITreeAttribute tree) continue;

                    tree.SetDouble("createdTotalHours", api.World.Calendar.TotalHours);
                    tree.SetDouble("lastUpdatedTotalHours", api.World.Calendar.TotalHours);

                    var freshHoursAttr = tree["freshHours"] as FloatArrayAttribute;
                    float freshHours = perishProps.FreshHours.nextFloat(1f, api.World.Rand);
                    freshHoursAttr.value[transitionIndex] = freshHours;

                    var transitionHoursAttr = tree["transitionHours"] as FloatArrayAttribute;
                    float transitionHours = perishProps.TransitionHours.nextFloat(1f, api.World.Rand);
                    transitionHoursAttr.value[transitionIndex] = transitionHours;

                    var transitionedHoursAttr = tree["transitionedHours"] as FloatArrayAttribute;
                    transitionedHoursAttr.value[transitionIndex] = spoilageRel > 0f ?
                        freshHours + Math.Max(0f, transitionHours * (spoilageRel * 0.6f) - 2f) :
                        Math.Max(0f, transitionedHoursRelative * (0.8f + (float)(2 + quantity) * spoilageRelMax) * (transitionHours + freshHours));
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(BlockCrock), nameof(BlockCrock.GetDummySlotForFirstPerishableStack))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixDummySlotSelector(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var methodToFind = AccessTools.Method(typeof(CollectibleObject), nameof(CollectibleObject.GetTransitionableProperties));

            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(methodToFind)) continue;

                codes.Insert(i + 1, new(OpCodes.Call, AccessTools.Method(typeof(FixFreshness), nameof(FilterPerishableTransitions))));
                break;
            }

            return codes;
        }

        public static TransitionableProperties[] FilterPerishableTransitions(TransitionableProperties[] transitions) => transitions?.Where(trans => trans.Type == EnumTransitionType.Perish).ToArray();
    }
}

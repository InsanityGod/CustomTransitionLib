using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "addProcessesIntoInfo")]
    public static class AppendTransitionToHandbook
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(AppendTransitionToHandbook), nameof(AddProccessIntoInfoToHandbook));

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Switch && codes[i-3].operand == AccessTools.Field(typeof(TransitionableProperties), nameof(TransitionableProperties.Type)))
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_2),
                        new(OpCodes.Ldarg_S, 5),
                        new(OpCodes.Ldloc_S, 25),
                        new(OpCodes.Ldarg_3),
                        new(OpCodes.Ldloc_S, 29),
                        new(OpCodes.Ldloca_S, 26),
                        new(OpCodes.Call, method),
                    });
                    break;
                }
            }

            return codes;
        }

        public static void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop, ref bool addedItemStack)
        {
            var handler = prop.Type.GetCustomHandler();

            if (handler == null) return;
            addedItemStack = true;

            handler.AddProccessIntoInfoToHandbook(capi, components, verticalSpace, openDetailPageFor, prop);
        }
    }
}

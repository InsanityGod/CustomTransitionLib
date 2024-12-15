using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleObject), "UpdateAndGetTransitionStatesNative")]
    public static class DisallowNegativeTransitionHours
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

             for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if(code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder pos 
                    && pos.LocalIndex == 18
                    && codes[i + 2].opcode == OpCodes.Stind_R4)
                {
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldc_R4, 0f));
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), new Type[] { typeof(float), typeof(float) })));
                    break;
                }
            }

            return codes;
        }
    }
}

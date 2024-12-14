using CustomTransitionLib;
using HarmonyLib;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace BrainFreeze.HarmonyPatches
{
    [HarmonyPatch]
    public static class TrulyDirtyFixOnEnumParser
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            Type enumUtilsType = AccessTools.TypeByName("Newtonsoft.Json.Utilities.EnumUtils");

            yield return enumUtilsType.GetMethod("ParseEnum");
        }

        public static bool Prefix(Type enumType, string value, ref object __result)
        {
            if(enumType != typeof(EnumTransitionType)) return true;

            // Check if the value exists in the enum

            if (Array.Exists(Enum.GetNames(enumType), name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase))) return true;

            __result = CustomTransitionLibModSystem.Instance.FindFakeNumberForEnumName(value) ?? throw new ArgumentException($"Requested value '{value}' was not found.");

            return false; //Prevent default execution
        }
    }
}

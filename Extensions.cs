using CustomTransitionLib.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib
{
    public static class Extensions
    {
        public static T ConvertToCustom<T>(this EnumTransitionType val) where T : Enum => CustomTransitionLibModSystem.Instance.ConvertToCustom<T>(val);
        public static EnumTransitionType ConvertToFake<T>(this T val) where T : Enum => CustomTransitionLibModSystem.Instance.ConvertToFake(val);

        public static ICustomTransitionHandler GetCustomHandler(this EnumTransitionType val) => CustomTransitionLibModSystem.Instance.FindTransitionHandler(val);
    }
}

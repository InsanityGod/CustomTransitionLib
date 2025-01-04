using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace CustomTransitionLib.interfaces
{
    public interface ITransitionTypeEnumConverter
    {
        T ConvertToCustom<T>(EnumTransitionType val) where T : Enum;
        EnumTransitionType ConvertToFake<T>(T val) where T : Enum;
        int? FindFakeNumberForEnumName(string value);
        Type FindRealType(EnumTransitionType val);
        ICustomTransitionHandler FindTransitionHandler(EnumTransitionType val);

        void Register<C, T>()
            where C : ICustomTransitionHandler<T>, new()
            where T : Enum;

        void Register<T>(ICustomTransitionHandler<T> handler) where T : Enum;
    }
}

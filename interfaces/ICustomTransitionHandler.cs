using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace CustomTransitionLib.interfaces
{
    public interface ICustomTransitionHandler
    {
        public Type EnumType { get; }

        public float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, float currentResult);

        public float GetDefaultTransitionSpeedMul(EnumTransitionType transType);

        public void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling);
    }

    public interface ICustomTransitionHandler<in T> : ICustomTransitionHandler where T : Enum
    {
        /// <summary>
        /// This is only used for getting to the correct language keys
        /// </summary>
        public string ModId { get; }

        public string GetTransitionLangKey(T transType) => Enum.GetName(typeof(T), transType)?.ToLower();

        /// <summary>
        /// Just a quick way to get typeof(T) when you only have access to the non generic interface.
        /// You should not change this unless you want the mod to break
        /// </summary>
        Type ICustomTransitionHandler.EnumType => typeof(T);

        float ICustomTransitionHandler.GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, float currentResult) =>
            GetTransitionRateMul(world, inSlot, transType.ConvertToCustom<T>(), currentResult);

        float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, T transType, float currentResult) => currentResult;

        float ICustomTransitionHandler.GetDefaultTransitionSpeedMul(EnumTransitionType transType) =>
            GetDefaultTransitionSpeedMul(transType.ConvertToCustom<T>());

        float GetDefaultTransitionSpeedMul(T transType) => 1;

        void ICustomTransitionHandler.AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling) =>
            AppendAppendPerishableInfoText(state.Props.Type.ConvertToCustom<T>(), inSlot, dsc, world, state, nowSpoiling);

        public void AppendAppendPerishableInfoText(T transType, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
        {
            var transitionRate = inSlot.Itemstack.Collectible.GetTransitionRateMul(world, inSlot, transType.ConvertToFake());
            float transitionLevel = state.TransitionLevel;
			float freshHoursLeft = state.FreshHoursLeft / transitionRate;

            double hoursPerday = (double)world.Calendar.HoursPerDay;
            if(transitionLevel > 0f || freshHoursLeft <= 0f)
            {
                dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-progression", (int)Math.Round((double)(transitionLevel * 100f))));
            }
			else
            {
                if (transitionRate <= 0f)
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}", Array.Empty<object>()));
			    }
			    else if ((double)freshHoursLeft > hoursPerday)
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-duration-days", Math.Round((double)freshHoursLeft / hoursPerday, 1)));
			    }
			    else
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-duration-hours", Math.Round((double)freshHoursLeft, 1)));
			    }
            }
        }
    }
}
using Cairo;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace CustomTransitionLib.interfaces
{
    public interface ICustomTransitionHandler
    {
        public Type EnumType { get; }

        public float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, float currentResult);

        public float GetDefaultTransitionSpeedMul(EnumTransitionType transType);

        public void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling);
        public void PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, ref ItemStack result);
        public void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop);
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

        void ICustomTransitionHandler.PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, ref ItemStack result) => 
            PostOnTransitionNow(collectible, slot, props, props.Type.ConvertToCustom<T>(), ref result);

        public void PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, T transType, ref ItemStack result)
        {
            //Empty on purpose
        }

        float ICustomTransitionHandler.GetDefaultTransitionSpeedMul(EnumTransitionType transType) =>
            GetDefaultTransitionSpeedMul(transType.ConvertToCustom<T>());

        float GetDefaultTransitionSpeedMul(T transType) => 1;

        void ICustomTransitionHandler.AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling) =>
            AppendAppendPerishableInfoText(state.Props.Type.ConvertToCustom<T>(), inSlot, dsc, world, state, nowSpoiling);

        public void AppendAppendPerishableInfoText(T transType, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
        {
            var transitionRate = inSlot.Itemstack.Collectible.GetTransitionRateMul(world, inSlot, transType.ConvertToFake());
            float transitionLevel = state.TransitionLevel;
			float hoursLeft = (state.TransitionHours - (state.TransitionedHours - state.FreshHoursLeft)) / transitionRate;

            double hoursPerday = (double)world.Calendar.HoursPerDay;
            double hoursPerYear = world.Calendar.DaysPerYear * hoursPerday;
            if(transitionLevel > 0f && state.FreshHoursLeft <= 0 && transitionRate <= 0)
            {
                dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-progression", (int)Math.Round((double)(transitionLevel * 100f))));
            }
			else
            {
                if (transitionRate <= 0f)
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}", Array.Empty<object>()));
			    }
			    else if ((double)hoursLeft > hoursPerYear)
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-duration-years", Math.Round((double)hoursLeft / hoursPerYear, 1)));
			    }
                else if ((double)hoursLeft > hoursPerday)
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-duration-days", Math.Round((double)hoursLeft / hoursPerday, 1)));
			    }
			    else
			    {
			    	dsc.AppendLine(Lang.Get($"{ModId}:itemstack-{GetTransitionLangKey(transType)}-duration-hours", Math.Round((double)hoursLeft, 1)));
			    }
            }
        }

        void ICustomTransitionHandler.AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop) =>
            AddProccessIntoInfoToHandbook(capi, components, verticalSpace, openDetailPageFor, prop, prop.Type.ConvertToCustom<T>());

        public void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop, T transType)
        {
            var transCode = GetTransitionLangKey(transType);
            
			components.Add(verticalSpace);
			components.Add(new RichTextComponent(capi, Lang.Get($"{ModId}:itemstack-{transCode}-handbook", prop.TransitionHours.avg) + "\n", CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold)));

			components.Add(
                new ItemstackTextComponent(
                    capi,
                    prop.TransitionedStack.ResolvedItemstack,
                    40.0,
                    10.0,
                    EnumFloat.Inline, 
                    stack => openDetailPageFor(GuiHandbookItemStackPage.PageCodeForStack(stack))
                )
                {
                    PaddingLeft = 2.0
			    }
            );
        }
    }
}
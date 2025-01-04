using CustomTransitionLib.interfaces;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace CustomTransitionLib
{
    public class CustomTransitionLibModSystem : ModSystem, ITransitionTypeEnumConverter
    {
        
        private Harmony harmony;

        public CustomTransitionLibModSystem()
        {
            Instance = this;
            currentOffset = Enum.GetValues(typeof(EnumTransitionType)).Cast<int>().Max() + 1; //Plus 1 since default is 0
        }

        public static ITransitionTypeEnumConverter Instance { get; private set; }

        private int currentOffset;

        /// <summary>
        /// Type to offsetFromBase
        /// </summary>
        private readonly OrderedDictionary<Type, int> OffsetLookup = new();

        private readonly Dictionary<Type, ICustomTransitionHandler> CustomTransitionHandlers = new();

        //TODO test performance!!
        public ICustomTransitionHandler FindTransitionHandler(EnumTransitionType val) => CustomTransitionHandlers.TryGetValue(FindRealType(val), out var result) ? result : null;

        public Type FindRealType(EnumTransitionType val)
        {
            Type type = null;
            foreach (var handler in OffsetLookup)
            {
                //Stop when we see we have gone outside the custom enum range
                if((int)val < handler.Value) break;
                type = handler.Key;
            }
            return type ?? typeof(EnumTransitionType);
        }

        //TODO maybe attribute for auto registry

        //TODO some way of ensuring these are registered with the same offset each time? (in case mods get removed/added)
        public void Register<C, T>() where C : ICustomTransitionHandler<T>, new() where T : Enum => Register(new C());
        public void Register<T>(ICustomTransitionHandler<T> handler) where T : Enum
        {
            if (OffsetLookup.ContainsKey(typeof(T)) || CustomTransitionHandlers.ContainsKey(typeof(T))) return;

            CustomTransitionHandlers[typeof(T)] = handler;

            var offset = Enum.GetValues(typeof(T)).Cast<int>().Max() + 1;
            OffsetLookup[typeof(T)] = currentOffset;
            currentOffset += offset;
            
        }

        /// <summary>
        /// Converts actual enum (with values over boundries) to custom enum
        /// </summary>
        /// <returns>The custom Enum contained within the default enum variable</returns>
        public T ConvertToCustom<T>(EnumTransitionType val) where T : Enum => (T)Enum.ToObject(typeof(T), (int)val - OffsetLookup[typeof(T)]);

        public EnumTransitionType ConvertToFake<T>(T val) where T : Enum => (EnumTransitionType)((int)Enum.ToObject(typeof(T), val) + OffsetLookup[typeof(T)]);

        public override void Start(ICoreAPI api)
        {
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();
            }

            base.Start(api);
        }


        public int? FindFakeNumberForEnumName(string value)
        {
            foreach ((var type, var offset) in OffsetLookup) 
            {
                var name =  Array.Find(Enum.GetNames(type), name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase));
                if(name == null) continue;

                return (int)Enum.Parse(type, name) + offset;
            }

            return null;
        }


        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            Instance = null;
            base.Dispose();
        }
    }
}
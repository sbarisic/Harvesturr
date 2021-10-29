using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Raylib_cs;
using System.Globalization;

namespace Harvesturr
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class IsGameToolAttribute : Attribute
    {
        public IsGameToolAttribute()
        {
        }

        public static IEnumerable<GameTool> CreateAllGameTools()
        {
            Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var T in AllTypes)
            {
                bool HasAttribute = T.GetCustomAttribute<IsGameToolAttribute>() != null;

                if (HasAttribute)
                {
                    GameTool ToolInstance = Activator.CreateInstance(T) as GameTool;
                    yield return ToolInstance;
                }
            }
        }
    }
}

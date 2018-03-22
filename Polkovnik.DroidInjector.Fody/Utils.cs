using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody
{
    internal static class Utils
    {
        public static bool IsActivity(this TypeDefinition typeDefinition)
        {
            var baseType = typeDefinition.BaseType;

            while (baseType != null)
            {
                if (baseType.FullName == "Android.App.Activity")
                {
                    return true;
                }

                baseType = baseType.Resolve().BaseType;
            }

            return false;
        }
    }
}

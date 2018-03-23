using System.Linq;
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

        public static FieldReference GetThisFieldReference(this FieldReference fieldReference)
        {
            if (!fieldReference.DeclaringType.GenericParameters.Any())
                return fieldReference;

            var declaringType = new GenericInstanceType(fieldReference.DeclaringType);
            foreach (var parameter in fieldReference.DeclaringType.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }
            
            return new FieldReference(fieldReference.Name, fieldReference.FieldType, declaringType);
        }
    }
}

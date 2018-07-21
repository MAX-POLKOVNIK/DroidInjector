using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Polkovnik.DroidInjector.Fody
{
    internal static class Utils
    {
        public static bool IsActivity(this TypeDefinition typeDefinition) => IsDerrivedFrom(typeDefinition, "Android.App.Activity");
        public static bool IsFragment(this TypeDefinition typeDefinition) => IsDerrivedFrom(typeDefinition, "Android.App.Fragment");
        public static bool IsSupportFragment(this TypeDefinition typeDefinition) => IsDerrivedFrom(typeDefinition, "Android.Support.V4.App.Fragment");
        public static bool IsRecyclerViewHolder(this TypeDefinition typeDefinition) => IsDerrivedFrom(typeDefinition, "Android.Support.V7.Widget.RecyclerView.ViewHolder");

        public static bool IsDerrivedFrom(this TypeDefinition typeDefinition, string typeFullName)
        {
            var baseType = typeDefinition.BaseType;

            while (baseType != null)
            {
                if (baseType.FullName == typeFullName)
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

        public static bool IsInAndroidClassLibrary(this IMemberDefinition memberDefinition)
        {
            var attribute = memberDefinition.DeclaringType.Module.Assembly.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == Consts.AndroidRuntimeResourceDesignerAttributeName);
            if (attribute == null)
                throw new WeavingException($"Attribute {Consts.AndroidRuntimeResourceDesignerAttributeName} not found. Are you in android project?");

            var isApplicationProperty = attribute.Properties.First(x => x.Name == "IsApplication");
            var isApplication = (bool)isApplicationProperty.Argument.Value;
            return !isApplication;
        }

        public static int GetResourceIdByName(string name, string fullName, TypeDefinition resourceIdClassType)
        {
            var field = GetResourceIdField(name, resourceIdClassType);

            if (field == null)
                throw new WeavingException($"Can't find id for member {fullName}.");

            if (!field.IsInAndroidClassLibrary())
                return (int)field.Constant;

            var staticCtor = resourceIdClassType.GetStaticConstructor();
            var stsfldInstruction = staticCtor.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Stsfld && ((FieldReference)x.Operand).Name == field.Name);
            if (stsfldInstruction == null)
                throw new WeavingException($"Can't find id for member {fullName}. Value in cctor not found");

            var previousOperation = stsfldInstruction.Previous;
            if (previousOperation.OpCode != OpCodes.Ldc_I4)
                throw new WeavingException($"Can't get id for member {fullName}. Instruction broken");

            return (int)previousOperation.Operand;
        }

        public static FieldDefinition GetResourceIdField(string name, TypeDefinition resourceIdClassType)
        {
            var constName = name.Trim('_');
            var field = resourceIdClassType.Fields.FirstOrDefault(x => x.Name == constName);
            return field;
        }
    }
}

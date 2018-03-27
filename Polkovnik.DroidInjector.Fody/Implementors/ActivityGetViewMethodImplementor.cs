using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal class ActivityGetViewMethodImplementor
    {
        private readonly ReferencesProvider _referencesProvider;
        private const string GetViewGeneratedMethodName = "Polkovnik_DroidInjector_GetRootView";

        private readonly TypeDefinition _typeDefinition;
        
        public ActivityGetViewMethodImplementor(TypeDefinition typeDefinition, ReferencesProvider referencesProvider)
        {
            _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }
        
        public MethodDefinition Execute()
        {
            Logger.LogExecute(this);

            var method = _typeDefinition.Methods.FirstOrDefault(x => x.Name == GetViewGeneratedMethodName);
            if (method != null)
            {
                Logger.Debug($"Type {_typeDefinition} already contains method {GetViewGeneratedMethodName}");
                return method;
            }

            Logger.Debug($"Adding method {GetViewGeneratedMethodName} into type {_typeDefinition}");

            var methodDefinition = new MethodDefinition(GetViewGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, 
                _referencesProvider.AndroidViewTypeReference);

            methodDefinition.Body.Variables.Add(new VariableDefinition(_referencesProvider.AndroidViewTypeReference));

            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0x01020002);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesProvider.ActivityFindViewByIdMethodReference);
            ilProcessor.Emit(OpCodes.Stloc_0);

            var ldloc0 = Instruction.Create(OpCodes.Ldloc_0);

            ilProcessor.Emit(OpCodes.Br_S, ldloc0);
            ilProcessor.Append(ldloc0);
            ilProcessor.Emit(OpCodes.Ret);

            return methodDefinition;
        }

        public override string ToString()
        {
            return $"{nameof(_referencesProvider)}: {_referencesProvider}, {nameof(_typeDefinition)}: {_typeDefinition}";
        }
    }
}

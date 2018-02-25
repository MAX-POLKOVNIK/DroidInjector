﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class ActivityGetViewMethodImplementor
    {
        private readonly ReferencesAndDefinitionsProvider _referencesAndDefinitionsProvider;
        private const string GetViewGeneratedMethodName = "Polkovnik_DroidInjector_GetRootView";

        private readonly TypeDefinition _typeDefinition;
        
        public ActivityGetViewMethodImplementor(TypeDefinition typeDefinition, ReferencesAndDefinitionsProvider referencesAndDefinitionsProvider)
        {
            _referencesAndDefinitionsProvider = referencesAndDefinitionsProvider ?? throw new ArgumentNullException(nameof(referencesAndDefinitionsProvider));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public bool IsNeedToAddGetViewMethod 
        {
            get
            {
                var baseType = _typeDefinition.BaseType;

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

        public MethodDefinition Execute()
        {
            var method = _typeDefinition.Methods.FirstOrDefault(x => x.Name == GetViewGeneratedMethodName);
            if (method != null)
            {
                Logger.Debug($"Type {_typeDefinition} already contains method {GetViewGeneratedMethodName}");
                return method;
            }

            Logger.Debug($"Adding method {GetViewGeneratedMethodName} into type {_typeDefinition}");

            var methodDefinition = new MethodDefinition(GetViewGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, 
                _referencesAndDefinitionsProvider.AndroidViewTypeReference);

            methodDefinition.Body.Variables.Add(new VariableDefinition(_referencesAndDefinitionsProvider.AndroidViewTypeReference));

            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0x01020002);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.ActivityFindViewByIdMethodReference);
            ilProcessor.Emit(OpCodes.Stloc_0);

            var ldloc0 = Instruction.Create(OpCodes.Ldloc_0);

            ilProcessor.Emit(OpCodes.Br_S, ldloc0);
            ilProcessor.Append(ldloc0);
            ilProcessor.Emit(OpCodes.Ret);

            return methodDefinition;
        }
    }
}

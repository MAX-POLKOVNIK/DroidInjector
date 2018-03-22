using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class InjectorCallReplacer
    {
        private readonly TypeDefinition _definition;
        private readonly string _methodNameToCall;
        private readonly MethodDefinition _methodToRemove;
        private readonly ActivityGetViewMethodImplementor _activityGetViewMethodImplementor;
        private readonly bool _methodIsParameterless;

        public InjectorCallReplacer(TypeDefinition definition, string methodNameToCall, MethodDefinition methodToRemove, ActivityGetViewMethodImplementor activityGetViewMethodImplementor)
        {
            _definition = definition;
            _methodNameToCall = methodNameToCall;
            _methodToRemove = methodToRemove;
            _activityGetViewMethodImplementor = activityGetViewMethodImplementor;
            _methodIsParameterless = !methodToRemove.HasParameters;
        }

        public void Execute()
        {
            var generatedMethod = _definition.Methods.First(x => x.Name == _methodNameToCall);
            
            MethodDefinition activityGetViewMethodDefinition = null;

            if (_definition.IsActivity())
            {
                activityGetViewMethodDefinition = _activityGetViewMethodImplementor.Execute();
            }

            foreach (var method in _definition.Methods)
            {
                if (!method.HasBody)
                    continue;

                while (true)
                {
                    var callInstuction = method.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call && IsMehodToRemove(x.Operand));
                    if (callInstuction == null)
                        break;

                    Logger.Debug($"Replace call {_methodToRemove} in {method.FullName}");
                    
                    if (_methodIsParameterless)
                    {
                        if (!_definition.IsActivity())
                            throw new FodyInjectorException($"Call Injector.InjectViews() in not activity class \"{_definition.FullName}\". Please pass view as parameter");

                        ReplaceParameterlessMethodInstructions(callInstuction, method.Body.GetILProcessor(), generatedMethod, activityGetViewMethodDefinition);
                    }
                    else
                    {
                        ReplaceMethodCallInsructions(callInstuction, method.Body.GetILProcessor(), generatedMethod);
                    }
                }
            }
            bool IsMehodToRemove(object operand) => operand is MethodReference methodReference && methodReference.Resolve() == _methodToRemove;
        }

        private static void ReplaceParameterlessMethodInstructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod, MethodReference getViewMethod)
        {
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Call, getViewMethod));
            ilProcessor.Replace(callInjectorInstruction, Instruction.Create(OpCodes.Call, injectionMethod));
        }

        /// <summary>
        /// Replaces call Polkovnik.DroidInjector.Injector::XXX(View).
        /// </summary>
        /// <param name="callInjectorInstruction">Instruction with calling Polkovnik.DroidInjector.Injector::XXX(View).</param>
        /// <param name="ilProcessor">Method body's ILProcessor.</param>
        /// <param name="injectionMethod">Activty's generated method for injecting.</param>
        private static void ReplaceMethodCallInsructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod)
        {
            var targetInstruction = callInjectorInstruction;

            while (targetInstruction.Previous.OpCode.Name.StartsWith("call"))
            {
                targetInstruction = targetInstruction.Previous;
            }

            while (targetInstruction.Previous.OpCode.Name.StartsWith("ld"))
            {
                targetInstruction = targetInstruction.Previous;
            }

            ilProcessor.InsertBefore(targetInstruction, Instruction.Create(OpCodes.Ldarg_0));

            ilProcessor.Replace(callInjectorInstruction, Instruction.Create(OpCodes.Call, injectionMethod));
        }
    }
}

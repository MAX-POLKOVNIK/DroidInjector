using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal sealed class InjectorCallsHarverster
    {
        internal sealed class HarvestedInstruction
        {
            public Instruction Instruction { get; set; }
            public MethodDefinition MethodDefinition { get; set; }
        }

        private readonly ModuleDefinition _moduleDefinition;

        public InjectorCallsHarverster(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }
        
        public HarvestedInstruction[] Execute()
        {
            Logger.LogExecute(this);

            var list = new List<HarvestedInstruction>();

            foreach (var typeDefinition in _moduleDefinition.Types)
            {
                foreach (var methodDefinition in typeDefinition.Methods.Where(x => x.HasBody))
                {
                    list.AddRange(methodDefinition.Body.Instructions.Where(instruction => instruction.Operand?.ToString().Contains("Injector::InjectViews") == true)
                                                                    .Select(instruction => new HarvestedInstruction
                                                                    {
                                                                        Instruction = instruction,
                                                                        MethodDefinition = methodDefinition
                                                                    }));
                }
            }

            return list.ToArray();
        }
    }
}

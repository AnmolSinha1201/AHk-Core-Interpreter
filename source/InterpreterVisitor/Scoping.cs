using static AHKCore.Nodes;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace AHKCore
{
	partial class InterpreterVisitor
	{
		BaseAHKNode scopeAndVariableOrFunction(dynamic context)
		{
			for (int i = 0; i < context.chain.Count; i++)
			{
				if (context.chain[i] is variableClass v)
				{
					if (indexed.Variables.Exists(v.variableName) && v.extraInfo is IndexedNode ind)
						indexed = ind;
					else if (indexed.Classes[v.variableName] != null)
						indexed = indexed.Classes[v.variableName];
					else
						return assemblyScope(context, i);
				}
			}

			switch (context)
			{
				case complexFunctionCallClass o:
					return scopeEndFunction(o);
				
				case complexVariableClass o:
					return scopeEndVariable(o);

				default:
					return null;
			}
		}

		BaseAHKNode scopeEndVariable(complexVariableClass context)
		{
			switch(context.variable)
			{
				case variableClass v:
					context.variable = variable(v);
				break;

				case dotUnwrapClass d:
					context.variable = variable((variableClass)d.variableOrFunction);
				break;
			}

			return context;
		}

		BaseAHKNode scopeEndFunction(complexFunctionCallClass context)
		{
			switch (context.function)
			{
				case functionCallClass f:
					context.function = functionCall(f);
				break;

				case dotUnwrapClass d:
					context.function = functionCall((functionCallClass)d.variableOrFunction);
				break;
			}

			return context;
		}
	}
}
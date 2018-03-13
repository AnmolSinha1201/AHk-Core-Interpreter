using System;
using System.Collections.Generic;
using System.Linq;
using static AHKCore.Nodes;
using static AHKCore.Query;

namespace AHKCore
{
	partial class InterpreterVisitor
	{
		public override functionCallClass functionCall(functionCallClass context)
		{
			var function = indexed.Functions[context.functionName][0];
			var oIndexed = indexed;
			indexed = new IndexedNode();

			var parameterVariableAssignList = addParams(context, function);
			function.functionBody = parameterVariableAssignList.Concat(function.functionBody).ToList();

			foreach (var functionNode in function.functionBody)
				traverser.objectDispatcher(functionNode);
			
			indexed = oIndexed;
			return context;
		}

		List<variableAssignClass> addParams(functionCallClass functionCall, functionDeclarationClass function)
		{
			var noDefaultList = addNoDefaultParams(functionCall, function.functionHead.functionParameters.Where(
				o => o.expression == null && o.isVariadic == false).ToList());

			var defaultParamList = addDefaultParams(functionCall, function.functionHead.functionParameters.Where(
				o => o.expression != null).ToList());

			return noDefaultList.Concat(defaultParamList).ToList();
		}

		List<variableAssignClass> addNoDefaultParams(functionCallClass functionCall, List<parameterInfoClass> functionParams)
		{
			var noDefaultParamList = new List<variableAssignClass>();
			foreach (var functionParam in functionParams)
			{
				if (functionCall.functionParameterList.Count == 0)
					break;
				noDefaultParamList.Add(assignVariable(functionParam.variableName, functionCall.functionParameterList[0]));
				functionCall.functionParameterList.RemoveAt(0);
			}

			return noDefaultParamList;
		}

		List<variableAssignClass> addDefaultParams(functionCallClass functionCall, List<parameterInfoClass> functionParams)
		{
			var defaultParamList = new List<variableAssignClass>();

			foreach (var functionParam in functionParams)
				defaultParamList.Add(assignVariable(functionParam.variableName, functionParam.expression));

			return defaultParamList.Concat(addNoDefaultParams(functionCall, functionParams)).ToList();
		}

		variableAssignClass assignVariable(variableClass name, BaseAHKNode expression)
		{
			return new variableAssignClass(new complexVariableClass(null, new List<BaseAHKNode>() {name}), "=", expression);
		}

		public override complexFunctionCallClass complexFunctionCall(complexFunctionCallClass context)
		{
			var oIndexed = indexed;
			foreach(var chainLink  in context.chain)
			{
				if (chainLink is variableClass v)
				{
					if (indexed.Variables[v.variableName] != null && v.extraInfo is IndexedNode i)
						indexed = i;
					else if (indexed.Classes[v.variableName] != null)
						indexed = indexed.Classes[v.variableName];
					else {} //throw error
				}
			}
			
			switch (context.function)
			{
				case functionCallClass f:
					context.function = functionCall(f);
				break;

				case dotUnwrapClass d:
					context.function = functionCall((functionCallClass)d.variableOrFunction);
				break;
			}

			indexed = oIndexed;
			return context;
		}
	}
}
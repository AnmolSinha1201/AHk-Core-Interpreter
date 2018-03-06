using System.Collections.Generic;
using System;

namespace AHKCore
{
	class Interpreter
	{
		public void Interpret()
		{
			var parserInstance = new Parser();
			var AHKNodes = parserInstance.parse("var1=123\nvar2=456\nvar1=var2");
			
			var indexer = new NodeIndexer();
			var indexedNodes = indexer.IndexNodes(AHKNodes);
			
			var visitor = new InterpreterVisitor();
			visitor.indexed = indexedNodes;
			var traverser = new AHKCore.NodeTraverser(visitor);

			// manually calling objectDispatcher which will call its visitor functions
			foreach (var o in indexedNodes.AutoExecute)
				traverser.objectDispatcher(o);
				
			Console.WriteLine(indexedNodes.Variables["var1"]);
		}
	}

	partial class InterpreterVisitor: AHKCore.BaseVisitor
	{
		//indexed will manage states
		public IndexedNode indexed;
	}
}
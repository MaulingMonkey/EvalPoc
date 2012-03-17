using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Threading;

namespace EvalPoc
{
	partial class Sandbox
	{
		public void Compile( string code )
		{
			try
			{
				var start = DateTime.Now;
				var results = Compiler.CompileAssemblyFromSource( CompilerParameters, code );

				CompilerResults = results;
				Assembly = results.CompiledAssembly;
			}
			catch ( Exception e )
			{
				CompilerException = e;
			}
		}

		private object CompilerMutex = new object();
		private Exception CompilerException;
		private CompilerResults CompilerResults;
		private Assembly Assembly;



		static readonly Dictionary<string,string> CompilerOptions = new Dictionary<string,string>()
		{
			{"CompilerVersion", "v3.5"},
		};
		static readonly CSharpCodeProvider Compiler = new CSharpCodeProvider(CompilerOptions);
		static readonly CompilerParameters CompilerParameters;

		static Sandbox()
		{
			// Scripts share the dependencies of the parent program:
			var assemblies = new List<string>();
			AddAssemblyPathsRecursively( assemblies, Assembly.GetEntryAssembly() );

			CompilerParameters = new CompilerParameters()
			{
				GenerateInMemory = true,
				IncludeDebugInformation = true,
			};
			CompilerParameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
		}

		static void AddAssemblyPathsRecursively( List<string> paths, Assembly assembly )
		{
			if (!paths.Contains(assembly.Location))
			{
				paths.Add(assembly.Location);
				foreach ( var referenced in assembly.GetReferencedAssemblies() )
				{
					AddAssemblyPathsRecursively( paths, Assembly.LoadWithPartialName( referenced.Name ) );
				}
			}
		}
	}
}

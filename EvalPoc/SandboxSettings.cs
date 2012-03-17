using System;
using System.Reflection;
using System.CodeDom.Compiler;

namespace EvalPoc
{
	class SandboxSettings
	{
		/// <summary>
		/// Per-compile timeout
		/// </summary>
		public TimeSpan CompileTimeOut     = TimeSpan.FromMilliseconds(200);

		/// <summary>
		/// Timeout for new sandboxed object construction
		/// </summary>
		public TimeSpan ConstructTimeOut   = TimeSpan.FromMilliseconds(200);

		/// <summary>
		/// Timeout for serializing existing sandboxes
		/// </summary>
		public TimeSpan SerializeTimeOut   = TimeSpan.FromMilliseconds(200);

		/// <summary>
		/// Timeout for unserializing existing sandboxes
		/// </summary>
		public TimeSpan UnserializeTimeOut = TimeSpan.FromMilliseconds(200);

		/// <summary>
		/// Per-frame runtime timeout
		/// </summary>
		public TimeSpan RunTimeOut         = TimeSpan.FromMilliseconds(200);


		/// <summary>
		/// N.B. Disabling this will cause the sandbox to ignore CompileTimeOut
		/// </summary>
		public bool CompileThread   = true;

		/// <summary>
		/// N.B. Disabling this will cause the sandbox to ignore ConstructTimeOut, SerializeTimeOut, and UnserializeTimeOut
		/// </summary>
		public bool ConstructThread = true;
		private bool SerializeThread { get { return ConstructThread; }}
		private bool UnserializeThread { get { return ConstructThread; }}

		/// <summary>
		/// N.B> Disabling this will cause the sandbox to ignore RunTimeOut
		/// </summary>
		public bool RunThread       = true;



		// Only one of these will be called:
		public Action<CompilerResults,Assembly> CompileSucceeded;
		public Action<CompilerResults> CompileFailed;
		public Action<Exception> CompileException; // includes timeouts

		// Only one of these will be called:
		public Action<IScript> CreationSucceeded;
		public Action<Exception> CreationException; // via new or deserialize, includes timeouts

		// Only one of these will be called;
		public Action RunSucceeded;
		public Action<Exception> RunException;
	}
}

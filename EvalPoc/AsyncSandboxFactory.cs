using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using SlimDX;

namespace EvalPoc
{
	/// <summary>
	/// Lets the UI spam rebuild requests while:
	///   1) Ensuring the build stays fresh by always 
	///   2) Not interrupting ongoing builds except for timeouts
	///   3) Discarding extra build requests that are outdated but haven't started building
	///   4) Handling most of the threading concerns itself.
	/// </summary>
	class AsyncSandboxFactory
	{
		readonly SandboxSettings Settings;
		readonly Thread Worker;

		struct Entry { public string Code; public Action<Sandbox> Callback; }

		readonly AutoResetEvent Build = new AutoResetEvent(false);
		readonly object Mutex = new object();
		/// <summary>
		/// Protect with Mutex!
		/// </summary>
		Entry Next;

		volatile bool Done = false;

		public AsyncSandboxFactory( SandboxSettings settings )
		{
			Settings = settings;
			Worker = new Thread(Work);
			Worker.Start();
		}

		public void Dispose()
		{
			Done = true;
			Build.Set();
		}

		private void WorkTimeout( Action action, TimeSpan timeout )
		{
			Exception threadEx = null;
			var thread = new Thread(()=>
			{
				try
				{
					action();
				}
				catch ( Exception ex )
				{
					threadEx = ex;
				}
			});
			thread.Start();
			bool r = thread.Join(timeout);
			if (!r)
			{
				thread.Abort();
				throw new TimeoutException("Thread timed out");
			}
			else if ( threadEx != null )
			{
				throw threadEx;
			}
		}

		private void Work()
		{
			while ( Build.WaitOne() && !Done )
			{
				Entry current;
				lock ( Mutex )
				{
					current = Next;
				}

				var sandbox = new Sandbox(Settings);

				try
				{
					//WorkTimeout( sandbox.Compile(current.Code), Settings.CompileTimeOut );
				}
				catch {}
			}
		}

		/// <summary>
		/// N.B. built will be called from an internal thread,
		/// you should probably have the callback call e.g.
		/// Control.BeginInvoke(...) or call MT-safe code.
		/// 
		/// Also note it may not be called at all if superceeded
		/// by a new Submit() before beginning compilation.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="built"></param>
		public void Submit( string code, Action<Sandbox> built, Action<Exception> exception )
		{
			lock ( Mutex )
			{
				Next = new Entry() { Code = code, Callback = built };
			}
			Build.Set();
		}
	}
}

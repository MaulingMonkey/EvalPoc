using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace EvalPoc
{
	public partial class EvalPocForm : Form
	{
		static readonly string AppDataDir			= Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EvalPoc" );
		static readonly string ScriptPath			= Path.Combine( AppDataDir, "LastScript.cs" );
		static readonly string SerializationPath	= Path.Combine( AppDataDir, "LastState.bin" );
		public EvalPocForm()
		{
			InitializeComponent();
			Application.Idle += new EventHandler(Application_Idle);

			var providerOptions = new Dictionary<string,string>()
			{
				{"CompilerVersion", "v3.5"},
			};

			Assemblies = new StringCollection();
			AddRecursively( Assemblies, Assembly.GetExecutingAssembly() );

			try
			{
				tbCode.Text = File.ReadAllText(ScriptPath);
			}
			catch ( DirectoryNotFoundException )
			{
				Directory.CreateDirectory(AppDataDir);
			}
			catch ( FileNotFoundException )
			{
			}

			CSCP = new CSharpCodeProvider(providerOptions);
		}

		void Application_Idle( object sender, EventArgs e ) {
			pbRenderTarget.Invalidate();
		}

		StringCollection Assemblies = null;
		CSharpCodeProvider CSCP = null;
		IScript LastCompiledScript = null;
		bool CodeModified = false;
		bool CodeExceptions = false;
		void CheckCodeModified()
		{
			if ( CodeModified )
			{
				CodeModified = false;
				ThreadPool.QueueUserWorkItem( Recompile, tbCode.Text );
			}
		}

		void ShowResultsAndRecheck( DateTime compileStart, CompilerResults results )
		{
			try
			{
				lvErrors.Items.Clear();

				bool errors = false;
				foreach ( CompilerError error in results.Errors )
				{
					var columns = new[]
						{ error.IsWarning ? "Compiler Warning" : "Compiler Error"
						, error.ErrorText
						};

					if ( !error.IsWarning ) errors = true;

					lvErrors.Items.Add( new ListViewItem(columns) );
				}

				if (!errors)
				{
					Type[] scripts = results.CompiledAssembly.GetTypes().Where(t=>t.GetInterfaces().Contains(typeof(IScript))).ToArray();

					switch ( scripts.Length )
					{
					case 0:
						lvErrors.Items.Add( new ListViewItem(new[]{"Reflection Error","Couldn't find any IScript implementing classes"}) );
						return;
					case 1:
						break; // OK
					default:
						lvErrors.Items.Add( new ListViewItem(new[]{"Reflection Error","Found multiple IScript implementing classes:"}) );
						foreach ( var script in scripts ) lvErrors.Items.Add(new ListViewItem(new[]{"",script.FullName}));
						return;
					}

					var scriptType = scripts[0];

					IScript next = null;
					var newT = new Thread(()=>{
						next = results.CompiledAssembly.CreateInstance(scriptType.FullName) as IScript;
					});

					newT.Start();
					if (!newT.Join(10000))
					{
						lvErrors.Items.Add( new ListViewItem(new[]{"Timeout", "Took more than 10 seconds to create new Script, aborting"}) );
						newT.Abort();
					}
					else
					{
						LastCompiledScript = next;
					}
				}
			}
			finally
			{
				var end = DateTime.Now;
				Text = (end-compileStart).TotalMilliseconds.ToString("F0") + " ms from compile to new script object or build errors";
			}
		}

		void AddRecursively( StringCollection paths, Assembly assembly )
		{
			if (!paths.Contains(assembly.Location))
			{
				paths.Add(assembly.Location);
				foreach ( var referenced in assembly.GetReferencedAssemblies() )
				{
					AddRecursively( paths, Assembly.LoadWithPartialName( referenced.Name ) );
				}
			}
		}

		void Recompile( object code_ )
		{
			var start = DateTime.Now;
			var code = (string)code_;

			var param = new CompilerParameters()
			{
				GenerateInMemory = true,
				IncludeDebugInformation = true,
			};
			param.ReferencedAssemblies.AddRange(Assemblies.Cast<string>().ToArray());
			var results = CSCP.CompileAssemblyFromSource( param, code );

			Action final = ()=>ShowResultsAndRecheck(start,results);
			BeginInvoke(final);
		}

		private void tbCode_TextChanged( object sender, EventArgs e )
		{
			if (!CodeModified)
			{
				lvErrors.Items.Clear();
				CodeModified = true;
				CodeExceptions = false;
				CheckCodeModified();
			}
			File.WriteAllText(ScriptPath,tbCode.Text);
		}

		private void pbRenderTarget_Paint( object sender, PaintEventArgs args ) {
			if ( LastCompiledScript != null ) try
			{
				using ( var bitmap = new Bitmap(ClientSize.Width,ClientSize.Height) )
				{
					using ( var fx = Graphics.FromImage(bitmap) )
					{
						var script = LastCompiledScript;
						var args2 = new PaintEventArgs(fx,ClientRectangle);
						Exception ex = null;
						var render = new Thread(()=>{
							try
							{
								script.Run(args);
							}
							catch ( Exception tex )
							{
								ex = tex;
							}
						});
						render.Start();
						if (!render.Join(200))
						{
							render.Interrupt();
							render.Abort();
							LastCompiledScript = new ErrorScript( Font, "Script timed out rendering" );
							LastCompiledScript.Run(args2);
						}
						else if ( ex != null )
						{
							throw ex;
						}
					}
					args.Graphics.DrawImage( bitmap, ClientRectangle );
				}
			}
			catch ( Exception e )
			{
				if (!CodeExceptions)
				{
					CodeExceptions = true;
					lvErrors.Items.Add( new ListViewItem(new[]{"Exception Type", e.GetType().Name}) );
					lvErrors.Items.Add( new ListViewItem(new[]{"Exception", e.Message}) );
				}
			}
		}
	}
}

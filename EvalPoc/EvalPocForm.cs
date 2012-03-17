using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
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
			CSCP = new CSharpCodeProvider(providerOptions);

			try
			{
				LastSerializedScript = new MemoryStream( File.ReadAllBytes(SerializationPath) );
			}
			catch ( DirectoryNotFoundException )
			{
				Directory.CreateDirectory(AppDataDir);
			}
			catch ( FileNotFoundException )
			{
			}

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
		}

		protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
		{
			TrySerialize();
			if ( LastSerializedScript != null ) File.WriteAllBytes( SerializationPath, LastSerializedScript.ToArray() );
			base.OnClosing(e);
		}

		void Application_Idle( object sender, EventArgs e ) {
			pbRenderTarget.Invalidate();
		}

		StringCollection Assemblies = null;
		CSharpCodeProvider CSCP = null;
		IScript LastCompiledScript = null;
		MemoryStream LastSerializedScript = null;

		bool CodeModified = false;
		bool CodeExceptions = false;

		void CheckCodeModified()
		{
			if ( CodeModified )
			{
				TrySerialize();
				CodeModified = false;
				ThreadPool.QueueUserWorkItem( Recompile, tbCode.Text );
			}
		}

		void TrySerialize()
		{
			if ( LastCompiledScript == null ) return;

			try
			{
				var ms = new MemoryStream();
				var bf = new BinaryFormatter()
					{
					};
				bf.Serialize( ms, LastCompiledScript );
				ms.Position = 0;
				LastSerializedScript = ms;
			}
			catch ( Exception ex )
			{
				lvErrors.Items.Add( new ListViewItem(new[]{"Serialization Exception","Couldn't serialize script"}));
				lvErrors.Items.Add( new ListViewItem(new[]{"Exception Type", ex.GetType().FullName}) );
				lvErrors.Items.Add( new ListViewItem(new[]{"Exception Message", ex.Message}) );
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
					Exception ex = null;
					var newT = new Thread(()=>{
						try
						{
							if ( LastSerializedScript == null )
							{
								next = results.CompiledAssembly.CreateInstance(scriptType.FullName) as IScript;
							}
							else
							{
								LastSerializedScript.Position = 0;

								var bf = new BinaryFormatter()
									{ AssemblyFormat = FormatterAssemblyStyle.Simple // unnecessary?
									, Context = new StreamingContext( StreamingContextStates.File ) // unnecessary.
									, Binder = new SpecifiedAssemblyDeserializationBinder(results.CompiledAssembly)
									};
								next = bf.Deserialize(LastSerializedScript) as IScript;
							}
						}
						catch ( Exception e )
						{
							ex = e;
						}
					});

					newT.Start();
					if (!newT.Join(10000))
					{
						lvErrors.Items.Add( new ListViewItem(new[]{"Timeout", "Took more than 10 seconds to create new Script, aborting"}) );
						newT.Abort();
					}
					else if ( ex != null )
					{
						lvErrors.Items.Add( new ListViewItem(new[]{"Exception", "Exception creating script"}) );
						lvErrors.Items.Add( new ListViewItem(new[]{"Exception Type", ex.GetType().FullName}) );
						lvErrors.Items.Add( new ListViewItem(new[]{"Exception Message", ex.Message}) );
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
				var assembly = (results!=null && !results.Errors.HasErrors && results.CompiledAssembly!=null ) ? results.CompiledAssembly.GetName().Name : "N/A";
				Text = "AntFarm -- " + assembly + " -- " + (end-compileStart).TotalMilliseconds.ToString("F0") + " ms from compile to new script object or build errors";

				// evil hack to jump the new script object ahead by however long it took us to compile bringing us roughly in sync?
				PreviousFrameUtc -= (end-compileStart);
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

		float T = 0.0f;
		DateTime PreviousFrameUtc = DateTime.UtcNow;
		private void pbRenderTarget_Paint( object sender, PaintEventArgs args )
		{
			var nowUtc = DateTime.UtcNow;
			var dt = (float)(nowUtc-PreviousFrameUtc).TotalSeconds;
			PreviousFrameUtc = nowUtc;

			if ( dt<0 ) dt=0;
			if ( dt>1 ) dt=1;

			T += dt;



			if ( LastCompiledScript != null ) try
			{
				var w = pbRenderTarget.ClientSize.Width;
				var h = pbRenderTarget.ClientSize.Height;
				using ( var bitmap = new Bitmap(w,h) )
				{
					using ( var fx = Graphics.FromImage(bitmap) )
					{
						var script = LastCompiledScript;
						var args2 = new ScriptRunArgs(fx,new Rectangle(0,0,w,h),T,dt);
						Exception ex = null;
						var render = new Thread(()=>{
							try
							{
								script.Run(args2);
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
					args.Graphics.DrawImage( bitmap, new Rectangle(0,0,w,h) );
				}
			}
			catch ( Exception e )
			{
				if (!CodeExceptions)
				{
					CodeExceptions = true;
					lvErrors.Items.Add( new ListViewItem(new[]{"Exception", "Exception running script"}) );
					lvErrors.Items.Add( new ListViewItem(new[]{"Exception Type", e.GetType().FullName}) );
					lvErrors.Items.Add( new ListViewItem(new[]{"Exception Message", e.Message}) );
				}
			}
		}

		private void bNukeState_Click( object sender, EventArgs e ) {
			LastSerializedScript = null;
			ThreadPool.QueueUserWorkItem( Recompile, tbCode.Text );
		}

		private void EvalPocForm_Load( object sender, EventArgs e )
		{
			tbCode.Select(0,0);
		}
	}
}

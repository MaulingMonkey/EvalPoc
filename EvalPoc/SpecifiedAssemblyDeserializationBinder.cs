using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace EvalPoc
{
	class SpecifiedAssemblyDeserializationBinder : SerializationBinder
	{
		readonly Assembly Assembly;

		public SpecifiedAssemblyDeserializationBinder( Assembly assembly )
		{
			Assembly = assembly;
		}

		static readonly Regex reFQTN = new Regex(@", [^ ]+, Version=\d+.\d+.\d+.\d+, Culture=[^,]+, PublicKeyToken=[^]]+");

		private Type GetBasicName( string basicname )
		{
			return Type.GetType(basicname)
				?? Assembly.GetType(reFQTN.Replace(basicname,""),false)
				;
		}

		static readonly Regex reTypeNameStart      = new Regex(@"^[^\]\[,]+");
		static readonly Regex reFancyTypeNameStart = new Regex(@"^[^\]\[]+");

		private Type GetType( ref int seek, string fullname )
		{
			bool sq = fullname[seek] == '[';

			if ( sq ) ++seek;

			var m = (sq ? reFancyTypeNameStart : reTypeNameStart).Match(fullname.Substring(seek));
			Debug.Assert(m.Success);
			seek += m.Length;

			var basic = GetBasicName(m.Value);
			if ( basic == null ) return null;

			if ( basic.IsGenericType )
			{
				Debug.Assert(fullname[seek]=='[');
				++seek;
				var n = basic.GetGenericArguments().Length;
				var args = new Type[n];
				for ( int i=0 ; i<n ; ++i ) args[i] = GetType(ref seek,fullname);
				Debug.Assert(fullname[seek]==']');
				++seek;
				return basic.MakeGenericType(args);
			}
			else
			{
				return basic;
			}

			if ( sq )
			{
				Debug.Assert(fullname[seek] == ']');
				++seek;
			}
		}

		public override Type BindToType( string assembly, string type )
		{
			int seek = 0;
			return GetType(ref seek,type);
#if false
			//type += ", " + Assembly.GetName().Name;
			var noSpec = reFQTN.Replace(type,", "+Assembly.FullName+"]]");
			var a = Assembly.GetType(noSpec);
			if ( a != null ) return a;
			var c = Type.GetType(noSpec);
			if ( c != null ) return c;
			return null;
#endif
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AssignmentTests
{
	public class TestHelper
	{
		public static TempFileWrapper ExtractResourceToTempFile (string resourceName)
		{
			return ExtractResourceToTempFileWithPath (resourceName, Path.GetTempFileName ());
		}
		
		public static TempFileWrapper ExtractResourceToTempFileWithName (string resourceName, string targetName)
		{
			return ExtractResourceToTempFileWithPath (resourceName, Path.GetTempPath () + targetName);
		}
		
		public static TempFileWrapper ExtractResourceToTempFileWithPath (string resourceName, string targetPath)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
			
			if(resourceStream == null) {
				throw new Exception("BUG: The author forgot to set the resource " + resourceName + " to be embedded.");
			}
			
			StreamReader reader = new StreamReader(resourceStream);
			
			using(FileStream tempWriteStream = File.Open(targetPath, FileMode.OpenOrCreate)) {
				Byte[] text = new UTF8Encoding(true).GetBytes(reader.ReadToEnd());
				tempWriteStream.Write(text, 0, text.Length);
			}
			
			return new TempFileWrapper(targetPath);
		}
		
		public static List<string> ExtractResourceToLineArray (string resourceName) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
			StreamReader reader = new StreamReader(resourceStream);
			
			List<string> result = new List<string>();
			string line;
			while((line = reader.ReadLine ()) != null) {
				result.Add(line);
			}
			return result;
		}
		
		public static string JoinQuoted (string[] args)
		{
			string argStr = "";
			if(args != null) {
				foreach(string arg in args) {
					if(arg.IndexOf(' ') != -1) {
						argStr += '"' + arg + '"';
					} else {
						argStr += arg;
					}
					argStr += " ";
				}
			}
			return argStr;
		}
		
		public static Type LoadTypeFromAssembly (string typeName, Assembly assembly)
		{
			Type type = null;
			type = assembly.GetType (typeName);
			if(type == null) type = assembly.GetType (assembly.GetName ().Name + "." + typeName);
			if(type == null) type = assembly.GetType ((new List<Type> (assembly.GetTypes ()).ConvertAll<string> ((Type t) => t.FullName)).Find ((string s) => new Regex(typeName + "$").IsMatch (s)));
			return type;
		}
		
		public static Type LoadImplementationOfTypeFromAssembly (string interfaceName, Assembly assembly)
		{
			Type interfaceType = LoadTypeFromAssembly (interfaceName, assembly);
			
			List<Type> implementingTypes = new List<Type> (assembly.GetTypes ())
											.FindAll ((Type t) => t.IsClass)
											.FindAll ((Type t) => (new List<Type> (t.GetInterfaces ())).Contains (interfaceType));
			
			if (implementingTypes.Count == 1)
			{
				return implementingTypes[0];
			}
			else
			{
				throw new Exception ("Multiple implementations of interface type " + interfaceName);
				// TODO this may need to get smarter, if people do strange things with subclassing/factories
			}
		}
		
		public static dynamic CreateInstanceOfImplementationOfTypeFromAssembly (string interfaceName, Assembly assembly)
		{
			Type implementationType = LoadImplementationOfTypeFromAssembly (interfaceName, assembly);
			
			return Activator.CreateInstance (implementationType);
			
			/*
			ConstructorInfo ctor = implementationType.GetConstructor (new Type[] {});
			if (ctor == null) {
				throw new Exception ("No constructor for implementation of type " + interfaceName + " with no arguments");
			}
			
			dynamic obj = ctor.Invoke (new Object[] {});
			return obj;
			*/
		}
		
		public static dynamic InvokeDynamicMethod (dynamic obj, string methodName, params object[] args) {
			Type objType = obj.GetType ();
			
			MethodInfo method = objType.GetMethod (methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(method != null) {
				return method.Invoke (obj, args);
			} else {
				PropertyInfo property = objType.GetProperty (methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if(property != null) {
					return property.GetValue (obj, args);
				} else {
					throw new Exception ("Failed to invoke method " + methodName);
				}
			}
		}
	}
	
	public class TempFileWrapper : IDisposable
	{
		public string Path {get; private set;}
		
		public TempFileWrapper(string path) {
			this.Path = path;
		}
		
		void IDisposable.Dispose() {
			try{
				File.Delete(this.Path);
			} catch (IOException) {
				// Do nothing
			}
		}
		
		public override string ToString() {
			return this.Path;
		}
		
		public static implicit operator string(TempFileWrapper t) {
			return t.ToString ();
		}
	}
}


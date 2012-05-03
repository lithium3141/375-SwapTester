using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AssignmentTests
{
	[TestFixture()]
	public class T3Test : ATest
	{
		[Test()]
		public void TestUniqueNames ()
		{
			string resource = "AssignmentTests.Resources.t3-samename.in";
			using (TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile(resource))
			{
				this.Runner.StartApp (new string[] {inFile});
				this.Runner.WriteInputLine("");
				
				List<string> lines = this.Runner.GetOutputLines ();
				
				List<string> errorLines = lines.FindAll ((string s) => (new Regex("^[Ee]rror")).IsMatch (s));
				Assert.AreEqual (1, errorLines.Count, this.Runner.ExtendedMessage ().WithMessages ("Unexpected number of errors detected in file", "This test should detect exactly one error"));
			}
		}
		
		[Test()]
		public void TestInterfaceExistence ()
		{
			Assembly appAssembly = this.Runner.LoadApplicationAssembly ();
			if(appAssembly == null)
			{
				Assert.Inconclusive ("Could not load application assembly\nPlease verify interfaces manually");
			}
			
			Dictionary<string, List<string>> expectedInterfaces = new Dictionary<string, List<string>> ();
			expectedInterfaces.Add ("LoanType", new List<string> () {"NONE", "FRM", "ARM"});
			expectedInterfaces.Add ("ITransaction", new List<string> () {"Name", "Type", "FaceValue", "CurrentFace", "IssueDate", "MaturityDate", "CouponRate", "MarginRate", "PrepayRate", "CreditRating"});
			expectedInterfaces.Add ("ITransactionLoader", new List<string> () {"GetTransactionsFromFiles", "GetTransactionsFromText"});
			foreach(KeyValuePair<string, List<string>> kvp in expectedInterfaces)
			{
				string expectedInterfaceName = kvp.Key;
				List<string> expectedInterfaceMethods = kvp.Value;
				
				Type type = TestHelper.LoadTypeFromAssembly (expectedInterfaceName, appAssembly);
				Assert.IsNotNull (type, "Test loaded null type for name " + expectedInterfaceName);
				
				List<string> propertyNames;
				if(!expectedInterfaceName.Equals ("LoanType"))
				{
					Assert.IsTrue (type.IsInterface, "Type " + type.Name + " is not an interface type");
					
					propertyNames = new List<string> ();
					propertyNames.AddRange ((new List<PropertyInfo> (type.GetProperties ())).ConvertAll<string> ((PropertyInfo pi) => pi.Name));
					propertyNames.AddRange ((new List<MethodInfo> (type.GetMethods ())).ConvertAll<string> ((MethodInfo mi) => mi.Name));
				}
				else
				{
					Assert.IsTrue (type.IsEnum, "Type " + type.Name + " is not an enum type");
					
					propertyNames = new List<string> (Enum.GetNames (type));
				}
				
				foreach(string expectedMethodName in expectedInterfaceMethods)
				{
					Assert.IsTrue (propertyNames.Contains (expectedMethodName), "Type " + type.Name + " does not have method with name " + expectedMethodName);
				}
			}
		}
		
		[Test()]
		public void TestUsesShortDates ()
		{
			Assert.Ignore ("Not implemented");
		}
		
		[Test()]
		public void TestZeroErrors ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile("AssignmentTests.Resources.t3-zeroerror.in")) {
				this.Runner.StartApp(new string[] {inFile});
				this.Runner.WriteInputLine("");
				
				List<string> lines = this.Runner.GetOutputLines ();
				
				Assert.AreEqual (1, lines.FindAll ((string s) => (new Regex("[Ee]rror")).IsMatch (s)).Count,
				                 this.Runner.ExtendedMessage ().WithMessages ("Incorrect number of error reports", "This test should report exactly one error"));
				Assert.AreEqual (1, lines.FindAll ((string s) => (new Regex("ABC123")).IsMatch (s)).Count,
				                 this.Runner.ExtendedMessage ().WithMessages ("Incomplete error report", "Error report should include failing loan's name"));
				Assert.AreEqual (1, lines.FindAll ((string s) => (new Regex("(?:[Ff]ace )?[Vv]alue")).IsMatch (s)).Count,
				                 this.Runner.ExtendedMessage ().WithMessages ("Incomplete error report", "Error report should include the phrase 'face value'"));
			}
		}
		
		[Test()]
		public void TestCreditRiskParsing ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile("AssignmentTests.Resources.t3-creditrisk.in")) {
				this.Runner.StartApp(new string[] {inFile});
				this.Runner.WriteInputLine("");
				
				List<string> lines = this.Runner.GetOutputLines ();
				
				foreach(KeyValuePair<string,string> kvp in 
				       new Dictionary<string,string> {
							{"margin", "[Mm]argin"}, 
							{"credit rating", "[Cc]redit [Rr]at(?:ing|e)"}, 
							{"prepay rate", "[Pp]repay [Rr]ate"}
				}) {
					string phrase = kvp.Key;
					Regex regex = new Regex(kvp.Value);
					
					Assert.AreEqual (2, lines.FindAll ((string s) => regex.IsMatch (s)).Count, 
					                 this.Runner.ExtendedMessage ().WithMessages ("Failed to parse " + phrase, "Expected exactly two transactions with " + phrase + "field"));
				}
			}
		}
		
		[Test()]
		public void TestValidatesCreditRisk ()
		{
			Assert.Ignore ("Not implemented");
		}
		
		[Test()]
		public void TestLoanTypeParseFlexibility ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile("AssignmentTests.Resources.t3-loantype.in")) {
				this.Runner.StartApp(new string[] {inFile});
				this.Runner.WriteInputLine("");
				
				List<string> lines = this.Runner.GetOutputLines ();
				
				foreach(KeyValuePair<string,string> kvp in 
				       new Dictionary<string,string> {
							{"margin", "[Mm]argin"}, 
							{"credit rating", "[Cc]redit [Rr]at(?:ing|e)"}, 
							{"prepay rate", "[Pp]repay [Rr]ate"},
							{"ARM", "ARM"},
							{"FRM", "FRM"}
				}) {
					string phrase = kvp.Key;
					Regex regex = new Regex(kvp.Value);
					
					Assert.AreEqual (2, lines.FindAll ((string s) => regex.IsMatch (s)).Count, 
					                 this.Runner.ExtendedMessage ().WithMessages ("Failed to parse " + phrase, "Expected exactly two transactions with " + phrase + "field"));
				}
			}
		}
		
		[Test()]
		public void TestNewOutputFormat ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile("AssignmentTests.Resources.t3-loantype.in")) {
				this.Runner.StartApp(new string[] {"-csv", inFile});
				this.Runner.WriteInputLine("");
				
				List<string> lines = this.Runner.GetOutputLines ();
				
				List<string> expectedLines = TestHelper.ExtractResourceToLineArray("AssignmentTests.Resources.t3-loantype-csv.out");
				
				Assert.AreEqual (expectedLines.Count, lines.Count, this.Runner.ExtendedMessage ().WithMessage ("Unexpected number of lines in output"));
				for(int i = 0; i < lines.Count; i++) {
					Assert.AreEqual (expectedLines[i], lines[i], this.Runner.ExtendedMessage ().WithMessage ("Improper line in output"));
				}
			}
		}
		
		[Test()]
		public void TestNewOutputErrorFormat ()
		{
			Assert.Ignore ("Not implemented");
		}
	}
}


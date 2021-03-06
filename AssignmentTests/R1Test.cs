using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AssignmentTests
{
	[TestFixture()]
	public class R1Test : RTest
	{
		[Test()]
		public void TestEmpty ()
		{
			using(TempFileWrapper tempFile = TestHelper.ExtractResourceToTempFileWithName ("AssignmentTests.Resources.empty.txt", "empty range.txt")) {
				this.Runner.StartApp (new string[] {tempFile});
				
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				lines.RemoveAll ((string s) => (new Regex("^\\s*$")).IsMatch (s));
				Assert.AreEqual (1, lines.Count, "Unexpected number of lines in output");
				
				Regex firstLineRegex = new Regex("^[Rr]ange.*: .*$");
				Assert.IsTrue (firstLineRegex.Matches(lines[0]).Count > 0, 
				               this.Runner.ExtendedMessage ().WithMessage ("First line of text did not match"));
			}
		}
		
		[Test()]
		public void TestFull ()
		{
			using(TempFileWrapper tempFile = TestHelper.ExtractResourceToTempFileWithName ("AssignmentTests.Resources.r1-full.in", "test range.txt")) {
				this.Runner.StartApp (new string[] {tempFile});
				
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				lines.RemoveAll ((string s) => (new Regex("^\\s*$")).IsMatch (s));
				Assert.AreEqual (5, lines.Count, this.Runner.ExtendedMessage ().WithMessage ("Unexpected number of lines in output"));
				
				// TODO match the output more closely
			}
		}
	}
}


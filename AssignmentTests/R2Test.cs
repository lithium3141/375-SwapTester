using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AssignmentTests
{
	public enum R2OutputLineType {
		Whitespace,
		Header,
		Range,
		Error,
		Other
	};
	
	[TestFixture(), Timeout(2000)]
	public class R2Test : ATest
	{
		[Test()]
		public void TestBatchLoad ()
		{
			// Use the R1 input file multiple times
			// The temp file wrapper semantics will give different range names
			using(TempFileWrapper tempFile1 = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"),
			      tempFile2 = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"),
			      tempFile3 = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"))
			{
				this.Runner.StartApp (new string[] {tempFile1, tempFile2, tempFile3});
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (3, categorizedLines[R2OutputLineType.Header].Count, "Unexpected number of header lines printed");
				Assert.AreEqual (12, categorizedLines[R2OutputLineType.Range].Count, "Unexpected number of range lines printed");
			}
		}
		
		[Test()]
		public void TestFileOutput ()
		{
			using(TempFileWrapper outFile = new TempFileWrapper(Path.GetTempFileName ()),
			      inFile = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"))
			{
				this.Runner.StartApp (new string[] {"-output", outFile, inFile});
				
				List<string> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (0, categorizedLines[R2OutputLineType.Range].Count, "Program provided unexpected output");
				Assert.AreEqual (0, categorizedLines[R2OutputLineType.Header].Count, "Program provided unexpected output");
				
				using(FileStream outputReadStream = File.Open(outFile, FileMode.Open))
				{
					StreamReader reader = new StreamReader(outputReadStream);
					List<string> fileLines = new List<string>();
					String line;
					while((line = reader.ReadLine ()) != null) {
						fileLines.Add (line);
					}
					categorizedLines = this.CategorizeLines(fileLines);
					
					Assert.AreEqual (1, categorizedLines[R2OutputLineType.Header].Count, "Program had unexpected number of header lines in output file");
					Assert.AreEqual (4, categorizedLines[R2OutputLineType.Range].Count, "Program had unexpected number of range lines in output file");
				}
			}
		}
		
		[Test()]
		public void TestFileOutputWithBatch ()
		{
			using(TempFileWrapper outFile = new TempFileWrapper(Path.GetTempFileName ()),
			      inFile = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"),
			      inFile2 = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r1-full.in"))
			{
				this.Runner.StartApp (new string[] {"-output", outFile, inFile, inFile2});
				
				List<string> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (0, categorizedLines[R2OutputLineType.Range].Count, "Program provided unexpected output");
				Assert.AreEqual (0, categorizedLines[R2OutputLineType.Header].Count, "Program provided unexpected output");
				
				using(FileStream outputReadStream = File.Open(outFile, FileMode.Open))
				{
					StreamReader reader = new StreamReader(outputReadStream);
					List<string> fileLines = new List<string>();
					String line;
					while((line = reader.ReadLine ()) != null) {
						fileLines.Add (line);
					}
					categorizedLines = this.CategorizeLines(fileLines);
					
					Assert.AreEqual (2, categorizedLines[R2OutputLineType.Header].Count, "Program had unexpected number of header lines in output file");
					Assert.AreEqual (8, categorizedLines[R2OutputLineType.Range].Count, "Program had unexpected number of range lines in output file");
				}
			}
		}
		
		[Test()]
		public void TestNegativeRanges ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r2-negative.in"))
			{
				this.Runner.StartApp (new string[] {inFile});
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (1, categorizedLines[R2OutputLineType.Header].Count, "Unexpected number of header lines printed");
				Assert.AreEqual (3, categorizedLines[R2OutputLineType.Range].Count, "Unexpected number of range lines printed");
				
				int[] occurrences = new int[] {1, 2, 1};
				for(int i = 0; i < 3; i++) {
					Assert.AreEqual (occurrences[i], (new Regex("100")).Matches(categorizedLines[R2OutputLineType.Range][i]).Count, "Bad output in range entry " + (i + 1));
				}
			}
		}
		
		[Test()]
		public void TestExtraValues ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r2-extra.in"))
			{
				this.Runner.StartApp (new string[] {inFile});
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (1, categorizedLines[R2OutputLineType.Header].Count, "Unexpected number of header lines printed");
				Assert.AreEqual (4, categorizedLines[R2OutputLineType.Range].Count, "Unexpected number of range lines printed");
				
				Regex extraValueRegex = new Regex("\\s*\\.[0-9]+$");
				for(int i = 0; i < 4; i++) {
					Assert.AreEqual (1, extraValueRegex.Matches (categorizedLines[R2OutputLineType.Range][i]).Count, "Extra value not found in output line " + (i + 1));
				}
			}
		}
		
		[Test()]
		public void TestErrors ()
		{
			using(TempFileWrapper inFile = TestHelper.ExtractResourceToTempFile ("AssignmentTests.Resources.r2-error.in"))
			{
				this.Runner.StartApp (new string[] {inFile});
				this.Runner.WriteInputLine ("");
				
				List<String> lines = this.Runner.GetOutputLines ();
				Dictionary<R2OutputLineType,List<string>> categorizedLines = this.CategorizeLines (lines);
				
				Assert.AreEqual (1, categorizedLines[R2OutputLineType.Header].Count, "Unexpected number of header lines printed");
				Assert.AreEqual (0, categorizedLines[R2OutputLineType.Range].Count, "Unexpected number of range lines printed");
				Assert.AreEqual (3, categorizedLines[R2OutputLineType.Error].Count + categorizedLines[R2OutputLineType.Other].Count, "Unexpected number of error indicators printed");
				
				// TODO able to test for more specific errors?
			}
		}
		
		public Dictionary<R2OutputLineType,List<string>> CategorizeLines (List<string> lines)
		{
			Regex headerRegex = new Regex("^Range");
			Regex rangeRegex = new Regex("^\\[(-?[0-9]*)-(-?[0-9]*)\\)");
			Regex whitespaceRegex = new Regex("^\\s*$");
			Regex errorRegex = new Regex("^Error in file$");
			
			Dictionary<R2OutputLineType,List<string>> result = new Dictionary<R2OutputLineType, List<string>>();
			foreach (R2OutputLineType type in Enum.GetValues (typeof(R2OutputLineType))) {
				result.Add (type, new List<string>());
			}
			
			foreach (string line in lines)
			{
				if (headerRegex.Matches (line).Count > 0) result[R2OutputLineType.Header].Add (line);
				else if (rangeRegex.Matches (line).Count > 0) result[R2OutputLineType.Range].Add (line);
				else if (whitespaceRegex.Matches (line).Count > 0) result[R2OutputLineType.Whitespace].Add (line);
				else if (errorRegex.Matches (line).Count > 0) result[R2OutputLineType.Error].Add (line);
				else result[R2OutputLineType.Other].Add (line);
			}
			
			return result;
		}
	}
}


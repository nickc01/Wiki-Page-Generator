using System;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{

	/// <summary>
	/// Represents a class that has been extracted from a CS file
	/// </summary>
	public class ClassExtraction : ExtractedInfo
	{
		/// <summary>
		/// Pattern for detecting xml commented class definitions
		/// </summary>
		static readonly Regex XMLCommentsPattern = new Regex(@"([ \t]*?)(\/\/\/\s*<[\S\s]*?\/.+?>[\r\n]*)\s*?(?:\[.*?\][\r\n]*?\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|\b)*)(?:class|struct)\s*([\w\d]+?(?:<.+?>)?[\w\d]*?\b))", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting regular commented class definitions
		/// </summary>
		static readonly Regex RegularCommentsPattern = new Regex(@"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s*?(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)(?:class|struct)\s*([\w\d]+?(?:<.+?>)?.+?\b))", RegexOptions.Compiled);

		/// <summary>
		/// The header of the class (eg. "public static class TESTCLASS {")
		/// </summary>
		public string ClassHeader;

		/// <summary>
		/// The name of the class
		/// </summary>
		public string ClassName;

		/// <summary>
		/// Attempts to extract a class from the input file text. Returns null if it cannot be found
		/// </summary>
		public static ClassExtraction Extract(string fileContents, string className, out bool found)
		{
			found = true;
			var extraction = TestMatches(XMLCommentsPattern.Matches(fileContents), className, m => new ClassExtraction
			{
				CommentStuff = m.Groups[2].Value,
				CommentType = CommentType.XMLComments,
				ClassHeader = m.Groups[3].Value,
				ClassName = m.Groups[5].Value,
			});
			if (extraction != null)
			{
				return extraction;
			}

			extraction = TestMatches(RegularCommentsPattern.Matches(fileContents), className, m => new ClassExtraction
			{
				CommentStuff = m.Groups[1].Value,
				CommentType = CommentType.RegularComments,
				ClassHeader = m.Groups[2].Value,
				ClassName = m.Groups[4].Value,
			});
			if (extraction != null)
			{
				return extraction;
			}
			else
			{
				found = false;
				return new ClassExtraction
				{
					ClassHeader = "public class " + className,
					CommentStuff = "",
					CommentType = CommentType.NoComments,
					ClassName = className,
				};
			}
		}

		/// <summary>
		/// Test if one of the matches is a valid class extraction
		/// </summary>
		/// <param name="matches">The matches to check over</param>
		/// <param name="className">The name of the class to check for</param>
		/// <param name="assembler">The function used for taking the match info and converting it into a <see cref="ClassExtraction"/></param>
		/// <returns>Returns a valid class extraction. Returns null if none of the matches were valid</returns>
		static ClassExtraction TestMatches(MatchCollection matches, string className, Func<Match, ClassExtraction> assembler)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				var extraction = assembler(match);

				if ((extraction.ClassHeader.Contains("class") || extraction.ClassHeader.Contains("struct")) && extraction.ClassName == className)
				{
					return extraction;
				}
			}
			return null;
		}
	}
}

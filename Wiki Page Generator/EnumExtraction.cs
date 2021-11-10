using System;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Extracts a enum from a CS file
	/// </summary>
	public class EnumExtraction : ExtractedInfo
	{
		/// <summary>
		/// Pattern for detecting xml commented class definitions
		/// </summary>
		static readonly Regex XMLCommentsPattern = new Regex(@"([ \t]*?)(\/\/\/\s*<[\S\s]*?\/.+?>[\r\n]*)\s*?(?:\[.*?\][\r\n]*?\s*?)*(((public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|\b)*)(?:enum)\s*([\w\d]+?(?:<.+?>)?.+?)[\r\n]*)\s*?\{([\S\s]*?\})", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting regular commented class definitions
		/// </summary>
		static readonly Regex RegularCommentsPattern = new Regex(@"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s*?(((public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|\b)*)(?:enum)\s*([\w\d]+?(?:<.+?>)?.+?)[\r\n]*)\s*?\{([\S\s]*?\})", RegexOptions.Compiled);

		/// <summary>
		/// The header definition for the enum
		/// </summary>
		public string EnumHeader;

		/// <summary>
		/// The name of the enum
		/// </summary>
		public string EnumName;

		/// <summary>
		/// The content of the enum
		/// </summary>
		public string EnumContent;

		/// <summary>
		/// Attempts to extract an enum from the input file text
		/// </summary>
		public static EnumExtraction Extract(string fileContents, string enumName, out bool found)
		{
			found = true;
			var extraction = TestMatches(XMLCommentsPattern.Matches(fileContents), enumName, m => new EnumExtraction
			{
				CommentStuff = m.Groups[2].Value,
				CommentType = CommentType.XMLComments,
				EnumHeader = m.Groups[3].Value,
				EnumName = m.Groups[6].Value,
				EnumContent = m.Groups[7].Value
			});
			if (extraction != null)
			{
				return extraction;
			}

			extraction = TestMatches(RegularCommentsPattern.Matches(fileContents), enumName, m => new EnumExtraction
			{
				CommentStuff = m.Groups[1].Value,
				CommentType = CommentType.RegularComments,
				EnumHeader = m.Groups[2].Value,
				EnumName = m.Groups[5].Value,
				EnumContent = m.Groups[6].Value
			});
			if (extraction != null)
			{
				return extraction;
			}
			else
			{
				found = false;
				return new EnumExtraction
				{
					EnumHeader = "public enum " + enumName,
					CommentStuff = "",
					CommentType = CommentType.NoComments,
					EnumName = enumName,
					EnumContent = ""
				};
			}
		}

		/// <summary>
		/// Test if one of the matches is a valid enum extraction
		/// </summary>
		/// <param name="matches">The matches to check over</param>
		/// <param name="enumName">The name of the enum to check for</param>
		/// <param name="assembler">The function used for taking the match info and converting it into a <see cref="EnumExtraction"/></param>
		/// <returns>Returns a valid enum extraction. Returns null if none of the matches were valid</returns>
		static EnumExtraction TestMatches(MatchCollection matches, string enumName, Func<Match, EnumExtraction> assembler)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				var extraction = assembler(match);

				if (extraction.EnumHeader.Contains("enum") && extraction.EnumName == enumName)
				{
					return extraction;
				}
			}
			return null;
		}
	}
}

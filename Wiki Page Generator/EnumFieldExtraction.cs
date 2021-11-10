using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Represents a field value extracted from an enum type
	/// </summary>
	public class EnumFieldExtraction : ExtractedInfo
	{
		/// <summary>
		/// Pattern for detecting xml commented class definitions
		/// </summary>
		static readonly Regex XMLCommentsPattern = new Regex(@"([ \t]*?)(\/\/\/\s*<[\S\s]*?\/.+?>[\r\n]*)\s*?(?:\[.*?\][\r\n]*?\s*?)*\s*?[\w\d]*?([\w\d]+?\b)", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting regular commented class definitions
		/// </summary>
		static readonly Regex RegularCommentsPattern = new Regex(@"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s*?(?:\[.*?\][\r\n]*?\s*?)*\s*?[\w\d]*?([\w\d]+?\b)", RegexOptions.Compiled);

		/// <summary>
		/// The name of the enum value
		/// </summary>
		public string ValueName;

		/// <summary>
		/// Attempts to extract enum fields from the enum extraction
		/// </summary>
		public static List<EnumFieldExtraction> Extract(EnumExtraction extractedEnum)
		{
			List<EnumFieldExtraction> extractedFields = new List<EnumFieldExtraction>();

			TestMatches(XMLCommentsPattern.Matches(extractedEnum.EnumContent), extractedFields, m => new EnumFieldExtraction
			{
				CommentStuff = m.Groups[2].Value,
				CommentType = CommentType.XMLComments,
				ValueName = m.Groups[3].Value
			});

			TestMatches(RegularCommentsPattern.Matches(extractedEnum.EnumContent), extractedFields, m => new EnumFieldExtraction
			{
				CommentStuff = m.Groups[1].Value,
				CommentType = CommentType.RegularComments,
				ValueName = m.Groups[2].Value
			});

			return extractedFields;
		}

		/// <summary>
		/// Tests if the matches are valid, and adds them to the <paramref name="listToAddTo"/> of they are valid
		/// </summary>
		/// <param name="matches">The matches to check over</param>
		/// <param name="listToAddTo">A list of all the extracted fields. This method will add it's own extractions to this list</param>
		/// <param name="assembler">The function used for taking the match info and converting it into a <see cref="EnumFieldExtraction"/></param>
		static void TestMatches(MatchCollection matches, List<EnumFieldExtraction> listToAddTo, Func<Match, EnumFieldExtraction> assembler)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				var extraction = assembler(match);

				if (!listToAddTo.Any(e => e.ValueName == extraction.ValueName))
				{
					listToAddTo.Add(extraction);
				}
			}
		}
	}
}

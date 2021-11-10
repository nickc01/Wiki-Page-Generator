using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Represents a field that has been extracted from a CS file
	/// </summary>
	public class FieldExtraction : ExtractedInfo
	{
		/// <summary>
		/// Pattern for detecting tooltip commented field definitions
		/// </summary>
		static readonly Regex TooltipPattern = new Regex(@"([ \t]*?)(\[SerializeField\]\s +?) ?\[Tooltip\(\""(.+?)\""\)\]\s+?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting xml commented field definitions
		/// </summary>
		static readonly Regex XMLPattern = new Regex(@"([ \t]*?)(\/\/\/\s+?<summary>(?:\s*?\/\/\/.+?[\r\n]+?)*)\s*?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|\b)*)\b\s*?([\w\d]+?(?:<.+?>)?(?:\[.*?\])?)\s([\d\w]+?)(?:;|\s?{|\s?\=\>?)", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting tooltip commented field definitions (this specifically handles the case where the [Tooltip] attribute is defined before the [SerializedField] attribute)
		/// </summary>
		static readonly Regex TooltipInvertedPattern = new Regex(@"([ \t]*?)\[Tooltip\(\""(.+?)\""\)\]\s+?(\[SerializeField\]\s+?)?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting tooltip commented field definitions (this specifically handles the case where the [Tooltip] attribute is inlined with the [SerializedField] attribute, like in the example [SerializedField, Tooltip("This is a test comment")]
		/// </summary>
		static readonly Regex TooltipInlinePattern = new Regex(@"([ \t]*?)(\[SerializeField,\s+?)?Tooltip\(\""(.+?)\""\)\]\s*?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting regular commented field definitions
		/// </summary>
		static readonly Regex RegularCommentPattern = new Regex(@"([ \t]*?)(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]+\s+?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|\b)*)([\w\d]+?(?:\<.*?\>)?(?:\[.*?\])?)\s([\w\d\<\>,]+?)[\s;]", RegexOptions.Compiled);

		/// <summary>
		/// Is the field marked with the [SerializedField] attribute?
		/// </summary>
		public bool Serialized = false;

		/// <summary>
		/// The publicity markers on the field
		/// </summary>
		public string Publicity;
		
		/// <summary>
		/// The type of the field
		/// </summary>
		public string FieldType;

		/// <summary>
		/// The name of the field
		/// </summary>
		public string FieldName;

		/// <summary>
		/// How much indentation this field has
		/// </summary>
		public string Indentation;


		/// <summary>
		/// Attempts to extract fields from the input file text.
		/// </summary>
		public static List<FieldExtraction> Extract(string fileContents)
		{
			List<FieldExtraction> extractions = new List<FieldExtraction>();

			TestMatches(TooltipPattern.Matches(fileContents), extractions, match => new FieldExtraction
			{
				CommentType = CommentType.RegularComments,
				Serialized = match.Groups[2].Value.Contains("SerializeField"),
				CommentStuff = match.Groups[3].Value,
				Indentation = TabsToSpaces(match.Groups[1].Value),
				Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
				FieldType = match.Groups[5].Value,
				FieldName = match.Groups[6].Value
			});

			TestMatches(XMLPattern.Matches(fileContents), extractions, match => new FieldExtraction
			{
				CommentType = CommentType.XMLComments,
				Serialized = false,
				CommentStuff = match.Groups[2].Value,
				Indentation = TabsToSpaces(match.Groups[1].Value),
				Publicity = match.Groups[3].Success ? match.Groups[3].Value : "",
				FieldType = match.Groups[4].Value,
				FieldName = match.Groups[5].Value
			});

			TestMatches(TooltipInvertedPattern.Matches(fileContents), extractions, match => new FieldExtraction
			{
				CommentType = CommentType.RegularComments,
				Serialized = match.Groups[3].Value.Contains("SerializeField"),
				CommentStuff = match.Groups[2].Value,
				Indentation = TabsToSpaces(match.Groups[1].Value),
				Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
				FieldType = match.Groups[5].Value,
				FieldName = match.Groups[6].Value
			});

			TestMatches(TooltipInlinePattern.Matches(fileContents), extractions, match => new FieldExtraction
			{
				CommentType = CommentType.RegularComments,
				Serialized = match.Groups[2].Value.Contains("SerializeField"),
				CommentStuff = match.Groups[3].Value,
				Indentation = TabsToSpaces(match.Groups[1].Value),
				Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
				FieldType = match.Groups[5].Value,
				FieldName = match.Groups[6].Value
			});

			TestMatches(RegularCommentPattern.Matches(fileContents), extractions, match => new FieldExtraction
			{
				CommentType = CommentType.RegularComments,
				Serialized = false,
				CommentStuff = match.Groups[2].Value,
				Indentation = TabsToSpaces(match.Groups[1].Value),
				Publicity = match.Groups[3].Success ? match.Groups[3].Value : "",
				FieldType = match.Groups[4].Value,
				FieldName = match.Groups[5].Value
			});

			return extractions;
		}

		/// <summary>
		/// Tests if the matches are valid, and adds them to the <paramref name="listToAddTo"/> of they are valid
		/// </summary>
		/// <param name="matches">The matches to check over</param>
		/// <param name="listToAddTo">A list of all the extracted fields. This method will add it's own extractions to this list</param>
		/// <param name="assembler">The function used for taking the match info and converting it into a <see cref="FieldExtraction"/></param>
		static void TestMatches(MatchCollection matches, List<FieldExtraction> listToAddTo, Func<Match, FieldExtraction> assembler)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}
				if (match.Value.Contains("readonly"))
				{
					continue;
				}

				var extraction = assembler(match);

				if (extraction.FieldType != "new" && extraction.FieldType != "public" && extraction.FieldType != "private" && extraction.FieldType != "protected" && extraction.FieldType != "static" && extraction.FieldType != "override" && extraction.FieldType != "virtual" && !listToAddTo.Any(e => e.FieldName == extraction.FieldName))
				{
					listToAddTo.Add(extraction);
				}
			}
		}
	}
}

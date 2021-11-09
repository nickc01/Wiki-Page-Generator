using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Represents a method that has been extracted from a CS file
	/// </summary>
	public class MethodExtraction : ExtractedInfo
	{
		/// <summary>
		/// Pattern for detecting xml commented method definitions
		/// </summary>
		static readonly Regex XMLPattern = new Regex(@"((\/\/\/\s?.+?[\r\n]\s+?)+)(?:\[.+?\]\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d]+?(?:\<.+?\>)?)\(.*?\))[\r\n]*?\s*?\{", RegexOptions.Compiled);
		/// <summary>
		/// Pattern for detecting regular commented method definitions
		/// </summary>
		static readonly Regex RegularCommentPattern = new Regex(@"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s+?(?:\[.+?\]\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d\<\>,]+?)\(.*?\))[\r\n]*?\s*?\{", RegexOptions.Compiled);

		/// <summary>
		/// Pattern for detecting method definitions (ignoring comments)
		/// </summary>
		static readonly Regex NoCommentsPattern = new Regex(@"([ \t]*?)(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d]+?(?:\<.+?\>)?)\(.*?\))[\r\n]*?\s*?\{", RegexOptions.Compiled);

		/// <summary>
		/// The publicility attributes on the method
		/// </summary>
		public string Visibility;

		/// <summary>
		/// The return type of the method
		/// </summary>
		public string ReturnType;

		/// <summary>
		/// The name of the method
		/// </summary>
		public string MethodName;

		/// <summary>
		/// The entire method definition
		/// </summary>
		public string MethodHeader;

		/// <summary>
		/// The amount of indentation on the method
		/// </summary>
		public string Indentation;

		/// <summary>
		/// Prunes comments by removing all of the ugly XML stuff so it's much easier to read online
		/// </summary>
		public override MethodCommentInfo PruneComments()
		{
			MethodCommentInfo comment = new MethodCommentInfo();
			if (string.IsNullOrEmpty(CommentStuff))
			{
				if (MethodName == "OnEnable")
				{
					comment.Comment = "Called when the script is enabled";
				}
				else if (MethodName == "OnDisable")
				{
					comment.Comment = "Called when the script is disabled";
				}
				else if (MethodName == "OnDestroy")
				{
					comment.Comment = "Called when the script is destroyed";
				}
				else if (MethodName == "Awake")
				{
					comment.Comment = "Called when the script starts";
				}
				else if (MethodName == "Awake")
				{
					comment.Comment = "Called when the script starts. This is called 1 frame after the Awake() function";
				}
				else if (MethodName == "Update")
				{
					comment.Comment = "Called once every frame";
				}
				else if (MethodName == "OnGameStart")
				{
					comment.Comment = "Called when the game starts";
				}
				else if (MethodName == "AfterSceneLoad")
				{
					comment.Comment = "Called after the first scene in the game loads";
				}
				else if (MethodName == "OnJoinedRoom")
				{
					comment.Comment = "Called when joining a photon room";
				}
				else if (MethodName == "OnPlayerEnteredRoom")
				{
					comment.Comment = "Called when a new player joins the photon room";
				}
			}
			if (string.IsNullOrEmpty(comment.Comment))
			{
				var baseResult = base.PruneComments();
				comment = new MethodCommentInfo
				{
					Comment = baseResult.Comment,
					parameterComments = new Dictionary<string, string>()
				};
			}

			var paramMatches = Regex.Matches(CommentStuff, @"<param name=""(.+?)"">([\S\s]+?)<\/param>");

			for (int i = 0; i < paramMatches.Count; i++)
			{
				var match = paramMatches[i];
				if (!comment.parameterComments.ContainsKey(match.Groups[1].Value))
				{
					comment.parameterComments.Add(match.Groups[1].Value, match.Groups[2].Value);
				}
			}

			return comment;
		}

		/// <summary>
		/// Attempts to extract methods from the input file text.
		/// </summary>
		public static List<MethodExtraction> Extract(string fileContents)
		{
			List<MethodExtraction> extractions = new List<MethodExtraction>();

			TestMatches(XMLPattern.Matches(fileContents), extractions, false, match => new MethodExtraction
			{
				CommentStuff = match.Groups[1].Value,
				CommentType = CommentType.XMLComments,
				MethodHeader = match.Groups[3].Value,
				Visibility = match.Groups[4].Value,
				ReturnType = match.Groups[5].Value,
				MethodName = ValidatePath(match.Groups[6].Value)
			});

			TestMatches(RegularCommentPattern.Matches(fileContents), extractions, false, match => new MethodExtraction
			{
				CommentStuff = match.Groups[1].Value,
				CommentType = CommentType.RegularComments,
				MethodHeader = match.Groups[2].Value,
				Visibility = match.Groups[3].Value,
				ReturnType = match.Groups[4].Value,
				MethodName = ValidatePath(match.Groups[5].Value),
			});

			TestMatches(NoCommentsPattern.Matches(fileContents), extractions, true, match => new MethodExtraction
			{
				CommentStuff = "",
				CommentType = CommentType.NoComments,
				Indentation = TabsToSpaces(match.Groups[1].Value ?? ""),
				MethodHeader = match.Groups[2].Value,
				Visibility = match.Groups[3].Value,
				ReturnType = match.Groups[4].Value,
				MethodName = ValidatePath(match.Groups[5].Value)
			});

			return extractions;
		}

		/// <summary>
		/// Tests if the matches are valid, and adds them to the <paramref name="listToAddTo"/> of they are valid
		/// </summary>
		/// <param name="matches">The matches to check over</param>
		/// <param name="listToAddTo">A list of all the extracted fields. This method will add it's own extractions to this list</param>
		/// <param name="refreshIndentation">Should this function refresh the indentation of all the methods in the <paramref name="listToAddTo"/> list</param>
		/// <param name="assembler">The function used for taking the match info and converting it into a <see cref="MethodExtraction"/></param>
		static void TestMatches(MatchCollection matches, List<MethodExtraction> listToAddTo, bool refreshIndentation, Func<Match, MethodExtraction> assembler)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}

				var extraction = assembler(match);

				if (extraction.ReturnType != "in" && extraction.ReturnType != "new" && extraction.ReturnType != "public" && extraction.ReturnType != "private" && extraction.ReturnType != "protected" && extraction.ReturnType != "static" && extraction.ReturnType != "override" && extraction.ReturnType != "virtual" && extraction.ReturnType != "class" && extraction.ReturnType != "struct" && !listToAddTo.Any(e => e.MethodHeader == extraction.MethodHeader))
				{
					listToAddTo.Add(extraction);
				}
				else if (refreshIndentation)
				{
					var existentElement = listToAddTo.FirstOrDefault(e => e.MethodHeader == extraction.MethodHeader);
					if (existentElement != null)
					{
						existentElement.Indentation = extraction.Indentation;
					}
				}
			}
		}
	}
}

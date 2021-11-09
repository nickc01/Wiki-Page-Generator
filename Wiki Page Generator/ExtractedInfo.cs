using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Common base class for anything that is extracted from a CS file
	/// </summary>
	public class ExtractedInfo
	{
		/// <summary>
		/// Converts all tabs into spaces
		/// </summary>
		/// <param name="input">The input string</param>
		/// <returns>Returns the same string but with tabs removed</returns>
		public static string TabsToSpaces(string input)
		{
			return Regex.Replace(input, "	", "    ");
		}

		/// <summary>
		/// Replaces "<" and ">" with "`" so it works better when uploaded to github
		/// </summary>
		/// <param name="input">The input string</param>
		/// <returns>Returns the modified string</returns>
		public static string ValidatePath(string input)
		{
			var result = input.Replace("<", "`");
			result = result.Replace(">", "`");
			result = result.Replace(" ", "");
			return result;
		}

		/// <summary>
		/// The comment related stuff that was found on the extracted code piece
		/// </summary>
		public string CommentStuff;

		/// <summary>
		/// What type of comment has been extracted?
		/// </summary>
		public CommentType CommentType;

		/// <summary>
		/// Prunes comments by removing all of the ugly XML stuff so it's much easier to read online
		/// </summary>
		public virtual CommentInfo PruneComments()
		{
			CommentInfo comment = new CommentInfo();
			if (CommentType == CommentType.XMLComments)
			{
				var mainMatch = Regex.Match(CommentStuff, @"<summary>([\S\s]+?)<\/summary>");
				if (mainMatch.Success)
				{
					comment.Comment = mainMatch.Groups[1].Value;
					comment.Comment = Regex.Replace(comment.Comment, @"\/\/\/[\t\s]*", @"");
					comment.Comment = comment.Comment.Trim();
					comment.Comment = Regex.Replace(comment.Comment, @"<see cref=""([\S\s]*?)""\/>", @"$1");
					comment.Comment = Regex.Replace(comment.Comment, @"<paramref name=""([\S\s]*?)""\/>", @"the $1 parameter ");
					comment.Comment = Regex.Replace(comment.Comment, @"([^\.])[\r\n][\s\t]*", @"$1 ");
					comment.Comment = Regex.Replace(comment.Comment, @"[\r\n][\s\t]*", @" ");
					comment.Comment = Regex.Replace(comment.Comment, @"<seealso cref=\""(.*?)\""\/>", @"$1");

				}
			}
			else if (CommentType == CommentType.RegularComments)
			{
				comment.Comment = CommentStuff.Trim();
			}

			return comment;
		}
	}
}

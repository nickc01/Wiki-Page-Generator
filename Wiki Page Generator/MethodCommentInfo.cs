using System.Collections.Generic;

namespace Wiki_Page_Generator
{

	/// <summary>
	/// Represents comment info specific to a method
	/// </summary>
	public class MethodCommentInfo : CommentInfo
	{
		/// <summary>
		/// A list of all the parameter comments on the method
		/// </summary>
		public Dictionary<string, string> parameterComments = new Dictionary<string, string>();
	}
}

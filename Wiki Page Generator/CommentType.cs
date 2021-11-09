namespace Wiki_Page_Generator
{
	/// <summary>
	/// What kind of comment are we dealing with?
	/// </summary>
	public enum CommentType
	{
		/// <summary>
		/// The comment is in the XML format
		/// </summary>
		XMLComments,
		/// <summary>
		/// The comment is in the regular, double slash format
		/// </summary>
		RegularComments,
		/// <summary>
		/// No comment was found on the code piece
		/// </summary>
		NoComments
	}
}

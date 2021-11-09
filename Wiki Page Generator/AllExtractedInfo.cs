using System.Collections.Generic;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// A collection of all the extracted info from a CS file
	/// </summary>
	public class AllExtractedInfo
	{
		/// <summary>
		/// All the extracted field info
		/// </summary>
		public List<FieldExtraction> Fields;

		/// <summary>
		/// All the extracted method info
		/// </summary>
		public List<MethodExtraction> Methods;

		/// <summary>
		/// All the extracted class info
		/// </summary>
		public ClassExtraction Class;
	}
}

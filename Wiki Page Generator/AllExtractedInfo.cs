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
		/// All the extracted class info. If this is null, then it may be an enum. If that's the case, the <see cref="Enum"/> variable may have a value set in it
		/// </summary>
		public ClassExtraction Class;

		/// <summary>
		/// The extracted enum info. This is only set if <see cref="Class"/> is null.
		/// </summary>
		public EnumExtraction Enum;
	}
}

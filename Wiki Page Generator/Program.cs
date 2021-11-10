using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiki_Page_Generator
{
	/// <summary>
	/// Main program for generating wiki page documentation
	/// </summary>
	class Program
	{
		const string WIKI_HEADER = @"## Description
*TYPE_DESCRIPTION*";

		const string WIKI_TABLE_HEADER = @"| *LEFT* | *RIGHT* |
| --- | --- |";

		const string WIKI_TABLE_ELEMENT = @"| *LEFT* | *RIGHT* |";

		const string LINKED_WIKI_TABLE_ELEMENT = @"| [*LINK_NAME*](*WIKI_PAGE**FILENAME*) | *RIGHT* |";


		const string WIKI_METHOD_PARAM_HEADER = @"| Parameters | Description |
| --- | --- |
";

		const string WIKI_METHOD_PARAM_ELEMENT = @"| *LEFT* | *RIGHT* |
";

		const string WIKI_METHOD_HEADER = @"## Function:
```cs
*FUNCTION_SIGNATURE*
```
## Description:
*FUNCTION_COMMENT*

Part of class [*CLASS_NAME*](*WIKI_PAGE**CLASS_FILE_NAME*)

";

		/// <summary>
		/// A list of all the pages that have been generated
		/// </summary>
		static ConcurrentDictionary<string, List<string>> totalPages = new ConcurrentDictionary<string, List<string>>();

		/// <summary>
		/// The link to the repository online
		/// </summary>
		static string repoLink;

		/// <summary>
		/// Main part of the program.
		/// USAGE:
		/// "Wiki Page Generator.exe" [REPO NAME] [DESTINATION DIRECTORY] [CS Script File 1] [CS Script File 2] [CS Script File 2] [etc...]
		/// </summary>
		static void Main(string[] args)
		{
			if (args.GetLength(0) == 0)
			{
				Console.WriteLine("ERROR : NO ARGUMENTS PASSED");
				return;
			}

			if (args.GetLength(0) == 1)
			{
				Console.WriteLine("ERROR : NO DESTINATION DIRECTORY SPECIFIED");
				return;
			}

			if (args.GetLength(0) == 2)
			{
				Console.WriteLine("ERROR : NO CS FILES SPECIFIED");
				return;
			}
			var watch = Stopwatch.StartNew();
			totalPages.Clear();

			repoLink = $"https://github.com/nickc01/{args[0]}/wiki/";

			var dumpLocation = new DirectoryInfo(args[1]);

			Parallel.For(2, args.Length, i =>
			{
				var extraction = ExtractAllInfo(args[i]);

				List<string> addedPages = new List<string>();

				if (extraction.Class != null)
				{
					CreateClassWikiPage(dumpLocation, extraction, addedPages);
				}
				else
				{
					CreateEnumWikiPage(dumpLocation, extraction, addedPages);
				}

				CreatePagesForMethods(dumpLocation, extraction, addedPages);

				totalPages.TryAdd(args[i], addedPages);
			});

			foreach (var key in totalPages.Keys.OrderBy(s => s))
			{
				foreach (var page in totalPages[key])
				{
					Console.WriteLine(page);
				}
			}
			watch.Stop();
			Console.WriteLine($"Time {watch.Elapsed.TotalSeconds}");
		}

		/// <summary>
		/// Creates pages for all the extracted methods of a class
		/// </summary>
		/// <param name="outputDir">The output directory the files will be placed in</param>
		/// <param name="info">The info that has been extracted from the CS file</param>
		/// <param name="addedPagesList">A list of all the pages that have been added. This method will add to this list when it adds its own page</param>
		static void CreatePagesForMethods(DirectoryInfo outputDir, AllExtractedInfo info, List<string> addedPagesList)
		{
			List<MethodExtraction> doneMethods = new List<MethodExtraction>();

			foreach (var method in info.Methods)
			{
				var similarMethods = info.Methods.Where(m => m.MethodName == method.MethodName).OrderByDescending(m => m.MethodHeader.Length);

				var largestMethod = similarMethods.First();
				if (doneMethods.Contains(largestMethod))
				{
					continue;
				}
				else
				{
					doneMethods.Add(largestMethod);
				}

				var comments = largestMethod.PruneComments();

				var builder = new StringBuilder(WIKI_METHOD_HEADER);
				builder.Replace("*FUNCTION_SIGNATURE*", largestMethod.MethodHeader);
				builder.Replace("*FUNCTION_COMMENT*", comments.Comment);
				builder.Replace("*CLASS_NAME*", info.Class.ClassName);
				builder.Replace("*WIKI_PAGE*", repoLink);
				builder.Replace("*CLASS_FILE_NAME*", info.Class.ClassName + "-(Script)");

				if (comments.parameterComments.Count > 0)
				{
					builder.Append(WIKI_METHOD_PARAM_HEADER);
					foreach (var param in comments.parameterComments)
					{
						builder.Append(WIKI_METHOD_PARAM_ELEMENT);
						builder.Replace("*LEFT*", param.Key);
						builder.Replace("*RIGHT*",param.Value);
					}
				}
				File.WriteAllText(outputDir.FullName + "\\" + info.Class.ClassName + "-" + largestMethod.MethodName + "-(Method).md", builder.ToString());

				addedPagesList.Add($"Page : {repoLink}{info.Class.ClassName}-{largestMethod.MethodName}-(Method)");
			}
		}

		/// <summary>
		/// Creates a page for the extracted enum
		/// </summary>
		/// <param name="outputDir">The output directory the files will be placed in</param>
		/// <param name="info">The info that has been extracted from the CS file</param>
		/// <param name="addedPagesList">A list of all the pages that have been added. This method will add to this list when it adds its own page</param>
		static void CreateEnumWikiPage(DirectoryInfo outputDir, AllExtractedInfo info, List<string> addedPagesList)
		{
			var builder = new StringBuilder(WIKI_HEADER);

			builder.Replace("*TYPE_DESCRIPTION*", info.Enum.PruneComments().Comment);
			builder.Append("\r\n\r\n");

			var enumFields = EnumFieldExtraction.Extract(info.Enum);

			if (enumFields.Count > 0)
			{
				builder.Append(WIKI_TABLE_HEADER);
				builder.Replace("*LEFT*", "Enum Values");
				builder.Replace("*RIGHT*", "Description");
				builder.Append("\r\n");

				foreach (var field in enumFields)
				{
					builder.Append(WIKI_TABLE_ELEMENT);
					builder.Replace("*LEFT*", field.ValueName);
					builder.Replace("*RIGHT*", field.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			File.WriteAllText(outputDir.FullName + "\\" + info.Enum.EnumName + "-(Script).md", builder.ToString());

			addedPagesList.Add($"Page : {repoLink}{info.Enum.EnumName}-(Script)");
		}

		/// <summary>
		/// Creates a page for the extracted class
		/// </summary>
		/// <param name="outputDir">The output directory the files will be placed in</param>
		/// <param name="info">The info that has been extracted from the CS file</param>
		/// <param name="addedPagesList">A list of all the pages that have been added. This method will add to this list when it adds its own page</param>
		static void CreateClassWikiPage(DirectoryInfo outputDir, AllExtractedInfo info, List<string> addedPagesList)
		{
			var builder = new StringBuilder(WIKI_HEADER);

			builder.Replace("*TYPE_DESCRIPTION*", info.Class.PruneComments().Comment);

			builder.Append("\r\n\r\n");

			var publicFields = info.Fields.Where(f => f.Publicity.Contains("public"));


			if (publicFields.Count() > 0)
			{
				builder.Append(WIKI_TABLE_HEADER);
				builder.Replace("*LEFT*", "Public Variables");
				builder.Replace("*RIGHT*", "Description");
				builder.Append("\r\n");
				foreach (var field in publicFields)
				{
					builder.Append(WIKI_TABLE_ELEMENT);
					builder.Replace("*LEFT*", field.FieldType + " " + field.FieldName);
					builder.Replace("*RIGHT*", field.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			builder.Append("\r\n\r\n");

			var privateFields = info.Fields.Where(f => !f.Publicity.Contains("public"));

			if (privateFields.Count() > 0)
			{
				builder.Append(WIKI_TABLE_HEADER);
				builder.Replace("*LEFT*", "Private Variables");
				builder.Replace("*RIGHT*", "Description");
				builder.Append("\r\n");
				foreach (var field in privateFields)
				{
					builder.Append(WIKI_TABLE_ELEMENT);
					builder.Replace("*LEFT*", field.FieldType + " " + field.FieldName);
					builder.Replace("*RIGHT*", field.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			builder.Append("\r\n\r\n");

			List<MethodExtraction> doneMethods = new List<MethodExtraction>();

			var publicMethods = info.Methods.Where(m => m.MethodHeader.Contains("public "));

			if (publicMethods.Count() > 0)
			{
				builder.Append(WIKI_TABLE_HEADER);
				builder.Replace("*LEFT*", "Public Functions");
				builder.Replace("*RIGHT*", "Description");
				builder.Append("\r\n");
				foreach (var method in publicMethods)
				{
					var similarMethods = info.Methods.Where(m => m.MethodName == method.MethodName).OrderByDescending(m => m.MethodHeader.Length);

					var largestMethod = similarMethods.First();
					if (doneMethods.Contains(largestMethod))
					{
						continue;
					}
					else
					{
						doneMethods.Add(largestMethod);
					}

					builder.Append(LINKED_WIKI_TABLE_ELEMENT);
					builder.Replace("*LINK_NAME*", largestMethod.ReturnType + " " + largestMethod.MethodName + "()");
					builder.Replace("*FILENAME*", info.Class.ClassName + "-" + largestMethod.MethodName + "-(Method)");
					builder.Replace("*WIKI_PAGE*", repoLink);
					builder.Replace("*RIGHT*", largestMethod.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			doneMethods.Clear();

			builder.Append("\r\n\r\n");

			var privateMethods = info.Methods.Where(m => !m.MethodHeader.Contains("public "));

			if (privateMethods.Count() > 0)
			{
				builder.Append(WIKI_TABLE_HEADER);
				builder.Replace("*LEFT*", "Private Functions");
				builder.Replace("*RIGHT*", "Description");
				builder.Append("\r\n");
				foreach (var method in privateMethods)
				{
					var similarMethods = info.Methods.Where(m => m.MethodName == method.MethodName).OrderByDescending(m => m.MethodHeader.Length);

					var largestMethod = similarMethods.First();
					if (doneMethods.Contains(largestMethod))
					{
						continue;
					}
					else
					{
						doneMethods.Add(largestMethod);
					}

					builder.Append(LINKED_WIKI_TABLE_ELEMENT);
					builder.Replace("*LINK_NAME*", largestMethod.ReturnType + " " + largestMethod.MethodName + "()");
					builder.Replace("*FILENAME*", info.Class.ClassName + "-" + largestMethod.MethodName + "-(Method)");
					builder.Replace("*WIKI_PAGE*", repoLink);
					builder.Replace("*RIGHT*", largestMethod.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			File.WriteAllText(outputDir.FullName + "\\" + info.Class.ClassName + "-(Script).md", builder.ToString());

			addedPagesList.Add($"Page : {repoLink}{info.Class.ClassName}-(Script)");
		}

		/// <summary>
		/// Extracts all possible class, method, and field information from a CS file
		/// </summary>
		/// <param name="fileLocation">The location of the file to open</param>
		/// <returns>Returns all extracted info from the file</returns>
		static AllExtractedInfo ExtractAllInfo(string fileLocation)
		{
			var file = new FileInfo(fileLocation);

			var fileContents = File.ReadAllText(fileLocation);

			var methods = MethodExtraction.Extract(fileContents);
			var fields = FieldExtraction.Extract(fileContents);

			var mostCommonIndentation = FindMostCommonIndent(methods);
			if (mostCommonIndentation != null)
			{
				fields.RemoveAll(f => f.Indentation != mostCommonIndentation);
			}

			var classInfo = ClassExtraction.Extract(fileContents, file.Name.Replace(file.Extension, ""),out var foundClass);

			EnumExtraction enumInfo = null;

			if (!foundClass)
			{
				enumInfo = EnumExtraction.Extract(fileContents, file.Name.Replace(file.Extension, ""), out var foundEnum);

				if (!foundEnum)
				{
					enumInfo = null;
				}
				else
				{
					classInfo = null;
				}
			}

			return new AllExtractedInfo
			{
				Fields = fields,
				Methods = methods,
				Class = classInfo,
				Enum = enumInfo
			};
		}

		/// <summary>
		/// Will look through all the extracted methods, and find the most common indentation level. This is used to verify what methods/fields are a part of the class definition
		/// </summary>
		/// <param name="extractions"></param>
		/// <returns></returns>
		static string FindMostCommonIndent(List<MethodExtraction> extractions)
		{
			var commonIndentations = new Dictionary<string, List<string>>();

			foreach (var e in extractions)
			{
				if (commonIndentations.TryGetValue(e.Indentation, out var list))
				{
					list.Add(e.Indentation);
				}
				else
				{
					var l = new List<string>();
					l.Add(e.Indentation);
					commonIndentations.Add(e.Indentation, l);
				}
			}

			string commonIndentation = null;
			int sizeOfList = -1;

			foreach (var indentation in commonIndentations)
			{
				if (commonIndentation == null)
				{
					commonIndentation = indentation.Key;
					sizeOfList = indentation.Value.Count;
				}
				else if (commonIndentation != indentation.Key && indentation.Value.Count > sizeOfList)
				{
					commonIndentation = indentation.Key;
					sizeOfList = indentation.Value.Count;
				}
			}
			return commonIndentation;
		}
	}
}

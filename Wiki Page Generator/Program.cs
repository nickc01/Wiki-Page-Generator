using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wiki_Page_Generator
{
	class Program
	{
		const string WIKI_HEADER = @"## Description
*TYPE_DESCRIPTION*";

		const string WIKI_TABLE_HEADER = @"| *LEFT* | *RIGHT* |
| --- | --- |";

		const string WIKI_TABLE_ELEMENT = @"| *LEFT* | *RIGHT* |";

		const string LINKED_WIKI_TABLE_ELEMENT = @"| [*LINK_NAME*](https://github.com/Darpaler/Escape-Room-AR/wiki/*FILENAME*) | *RIGHT* |";

		const string PAGE_LINK = @"https://github.com/Darpaler/Escape-Room-AR/wiki/";


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

Part of class [*CLASS_NAME*](https://github.com/Darpaler/Escape-Room-AR/wiki/*CLASS_FILE_NAME*)

";


		public enum CommentType
		{
			XMLComments,
			RegularComments,
			NoComments
		}

		class CSFileInfo
		{

		}

		class AllExtractedInfo
		{
			public List<FieldExtraction> Fields;
			public List<MethodExtraction> Methods;
			public ClassExtraction Class;
		}

		class CommentInfo
		{
			public string Comment = "";
		}

		class MethodCommentInfo : CommentInfo
		{
			public Dictionary<string, string> parameterComments = new Dictionary<string, string>();
		}

		class ClassExtraction
		{
			public string CommentStuff;
			public CommentType CommentType;
			public string ClassHeader;
			public string ClassName;

			public CommentInfo PruneComments()
			{
				CommentInfo comment = new CommentInfo();
				if (CommentType == CommentType.XMLComments)
				{
					var mainMatch = Regex.Match(CommentStuff, @"<summary>([\S\s]+?)<\/summary>");
					if (mainMatch.Success)
					{
						comment.Comment = mainMatch.Groups[1].Value;
						comment.Comment = Regex.Replace(comment.Comment, @"\/\/\/[\t\s]*",@"");
						comment.Comment = comment.Comment.Trim();
						comment.Comment = Regex.Replace(comment.Comment, @"<see cref=""([\S\s]*?)""\/>", @"``` $1 ```");
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

		class FieldExtraction
		{
			public CommentType CommentType;
			public string CommentStuff;
			public bool Serialized = false;
			public string Publicity;
			public string FieldType;
			public string FieldName;
			public string Indentation;

			public CommentInfo PruneComments()
			{
				CommentInfo comment = new CommentInfo();
				if (CommentType == CommentType.XMLComments)
				{
					var mainMatch = Regex.Match(CommentStuff, @"<summary>([\S\s]+?)<\/summary>");
					if (mainMatch.Success)
					{
						/*comment.Comment = mainMatch.Groups[1].Value;
						comment.Comment = comment.Comment.Replace("///", "");
						comment.Comment = comment.Comment.Trim();
						comment.Comment = Regex.Replace(comment.Comment, @"<see cref=""([\S\s]*?)""\/>", @"$1");
						comment.Comment = Regex.Replace(comment.Comment, @"<paramref name=""([\S\s]*?)""\/>", @"the $1 parameter ");
						comment.Comment = Regex.Replace(comment.Comment, @"^[\t\s]+", @"");*/

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

		class MethodExtraction
		{
			public string CommentStuff;
			public CommentType CommentType;

			public string Visibility;
			public string ReturnType;
			public string MethodName;
			public string MethodHeader;
			public string Indentation;

			public MethodCommentInfo PruneComments()
			{
				MethodCommentInfo comment = new MethodCommentInfo();

				//Console.WriteLine("Pruning Method = " + MethodName);
				//Console.WriteLine("Comment Stuff = " + CommentStuff);
				//Console.WriteLine("No COmment = " + string.IsNullOrEmpty(CommentStuff));
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

				if (CommentType == CommentType.XMLComments)
				{
					var mainMatch = Regex.Match(CommentStuff, @"<summary>([\S\s]+?)<\/summary>");
					if (mainMatch.Success)
					{
						/*comment.Comment = mainMatch.Groups[1].Value;
						comment.Comment = comment.Comment.Replace("///", "");
						comment.Comment = comment.Comment.Trim();
						comment.Comment = Regex.Replace(comment.Comment, @"<see cref=""([\S\s]*?)""\/>",@"$1");
						comment.Comment = Regex.Replace(comment.Comment, @"<paramref name=""([\S\s]*?)""\/>", @"the $1 parameter ");
						comment.Comment = Regex.Replace(comment.Comment, @"^[\t\s]+", @"");*/

						comment.Comment = mainMatch.Groups[1].Value;
						comment.Comment = Regex.Replace(comment.Comment, @"\/\/\/[\t\s]*", @"");
						comment.Comment = comment.Comment.Trim();
						comment.Comment = Regex.Replace(comment.Comment, @"<see cref=""([\S\s]*?)""\/>", @"$1");
						comment.Comment = Regex.Replace(comment.Comment, @"<paramref name=""([\S\s]*?)""\/>", @"the $1 parameter ");
						comment.Comment = Regex.Replace(comment.Comment, @"([^\.])[\r\n][\s\t]*", @"$1 ");
						comment.Comment = Regex.Replace(comment.Comment, @"[\r\n][\s\t]*", @" ");
						comment.Comment = Regex.Replace(comment.Comment, @"<seealso cref=\""(.*?)\""\/>", @"$1");

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
				}
				else if (CommentType == CommentType.RegularComments)
				{
					comment.Comment = CommentStuff.Trim();
				}

				return comment;
			}
		}

		static string TabsToSpaces(string input)
		{
			//return input.Replace("	", "   ");
			return Regex.Replace(input, @"\t", " ");
		}

		static string ValidatePath(string input)
		{
			var result = input.Replace("<", "`");
			result = result.Replace(">", "`");
			result = result.Replace(" ", "");
			return result;
		}

		static void Main(string[] args)
		{
			var dumpLocation = new DirectoryInfo(args[0]);

			for (int i = 1; i < args.Length; i++)
			{
				//var demoContent = File.ReadAllText(args[i]);

				//Console.WriteLine("EXTRACTING ALL INFO FROM FILE : " + args[i]);
				var extraction = ExtractAllInfo(args[i]);

				CreateClassWikiPage(dumpLocation, extraction);

				CreatePagesForMethods(dumpLocation, extraction);
			}

			//var demoContent = File.ReadAllText(args[0]);

			//var extractions = ExtractMethods(demoContent);

			//var fields = ExtractFields(demoContent);

			/*foreach (var method in extractions)
			{
				Console.WriteLine("Method = " + method.MethodHeader);
				Console.WriteLine("Comments = " + method.PruneComments().MethodComment);
				Console.WriteLine("I" + method.Indentation + "I");
			}

			var cIndent = FindMostCommonIndent(extractions);
			Console.WriteLine("COMMON INDENTATION = I" + cIndent + "I");
			Console.WriteLine("C_ILENGTH = " + cIndent.Length);*/
			//Console.WriteLine("Hello World!");


			//var info = ExtractAllInfo(demoContent);
			/*foreach (var field in fields)
			{
				Console.WriteLine("Field Name = " + field.FieldName);
				Console.WriteLine("Comment Stuff = " + field.CommentStuff);
				Console.WriteLine("INDENTATION = I" + field.Indentation + "I");
			}*/
		}

		static void CreatePagesForMethods(DirectoryInfo outputDir, AllExtractedInfo info)
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

				Console.WriteLine("PAGE : " + PAGE_LINK + info.Class.ClassName + "-" + largestMethod.MethodName + "-(Method)");
			}
		}

		static void CreateClassWikiPage(DirectoryInfo outputDir, AllExtractedInfo info)
		{
			//Console.WriteLine("CREATING PAGE : " + info.Class.ClassName + "-(Script)");
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

			//foreach (var field in info.Fields)
			//{
				
			//}

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
					builder.Replace("*RIGHT*", largestMethod.PruneComments().Comment);
					builder.Append("\r\n");
				}
			}

			File.WriteAllText(outputDir.FullName + "\\" + info.Class.ClassName + "-(Script).md", builder.ToString());

			Console.WriteLine("PAGE : " + PAGE_LINK + info.Class.ClassName + "-(Script)");
		}

		static AllExtractedInfo ExtractAllInfo(string fileLocation)
		{
			var file = new FileInfo(fileLocation);

			var fileContents = File.ReadAllText(fileLocation);

			//Console.WriteLine("EXTRACTING METHODS");
			var methods = ExtractMethods(fileContents);
			//Console.WriteLine("EXTRACTING FIELDS");
			var fields = ExtractFields(fileContents);

			//Console.WriteLine("A");
			var mostCommonIndentation = FindMostCommonIndent(methods);
			//Console.WriteLine("COMON INDENTATION = I" + mostCommonIndentation + "I");
			//Console.WriteLine("COMMON INDENTATION LENGTH = " + mostCommonIndentation.Length);
			//Console.WriteLine("B");
			fields.RemoveAll(f => f.Indentation != mostCommonIndentation);
			for (int i = fields.Count - 1; i >= 0; i--)
			{
				//Console.WriteLine("INDENTATION OF FIELD " + fields[i].FieldName + " = I" + fields[i].Indentation + "I");
				if (fields[i].Indentation != mostCommonIndentation)
				{
					//Console.WriteLine("LENGTH = " + fields[i].Indentation.Length);
					//Console.WriteLine("REMOVING FIELD = " + fields[i].FieldName);
				}
			}

			fields.RemoveAll(f => f.Indentation != mostCommonIndentation);

			//Console.WriteLine("C");

			//Console.WriteLine("D");
			foreach (var method in methods)
			{
				//Console.WriteLine("//" + method.PruneComments().MethodComment);
				//Console.WriteLine("Method = " + method.MethodHeader);
			}
			//Console.WriteLine("");
			//Console.WriteLine("E");
			//Console.WriteLine("EXTRACTING CLASS");
			var classInfo = ExtractClass(fileContents, file.Name.Replace(file.Extension,""));

			//Console.WriteLine("//" + classInfo.PruneComments().Comment);
			//Console.WriteLine("CLASS = " + classInfo.ClassHeader);


			return new AllExtractedInfo
			{
				Fields = fields,
				Methods = methods,
				Class = classInfo
			};
		}

		static ClassExtraction ExtractClass(string fileContents, string className)
		{
			//XML COMMENTS
			//OLD 1 : ([ \t]*?)((\/\/\/\s?.+?[\r\n]\s*?)+)((?:static\s)?(?:sealed\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:sealed\s)?(class\s|struct\s)([\d\w]+?))\s(?:\:\s*?.+?[\r\n])?{
			//OLD 2 : ([ \t]*?)(\/\/\/\s*<[\S\s]*?\/.+?>[\r\n]*)(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|\b)*)(?:class|struct)\s*([\w\d]+?(?:<.+?>)?.+?)[\r\n\s].+?[\r\n])
			var matches = Regex.Matches(fileContents, @"([ \t]*?)(\/\/\/\s*<[\S\s]*?\/.+?>[\r\n]*)\s*?(?:\[.*?\][\r\n]*?\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|\b)*)(?:class|struct)\s*([\w\d]+?(?:<.+?>)?.+?)[\r\n\s].+?[\r\n])");

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				if ((match.Groups[3].Value.Contains("class") || match.Groups[3].Value.Contains("struct")) && match.Groups[5].Value == className)
				{
					return new ClassExtraction
					{
						CommentStuff = match.Groups[2].Value,
						CommentType = CommentType.XMLComments,
						ClassHeader = match.Groups[3].Value,
						ClassName = className
					};
				}
			}

			//REGULAR COMMENTS

			//OLD 1 : (?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]((?:static\s)?(?:sealed\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:sealed\s)?(class\s|struct\s)([\d\w]+?))\s(?:\:\s*?.+?[\r\n])?{
			matches = Regex.Matches(fileContents, @"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n](((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)(?:class|struct)\s*([\w\d]+?(?:<.+?>)?.+?)[\r\n\s].+?[\r\n])");

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				if ((match.Groups[2].Value.Contains("class") || match.Groups[2].Value.Contains("struct")) && match.Groups[4].Value == className)
				{
					return new ClassExtraction
					{
						CommentStuff = match.Groups[1].Value,
						CommentType = CommentType.RegularComments,
						ClassHeader = match.Groups[2].Value,
						ClassName = className
					};
				}
			}

			return new ClassExtraction
			{
				ClassHeader = "public class " + className,
				CommentStuff = "",
				CommentType = CommentType.NoComments,
				ClassName = className
			};
		}

		static List<FieldExtraction> ExtractFields(string fileContents)
		{
			List<FieldExtraction> fields = new List<FieldExtraction>();

			//METHOD A : WITH TOOLTIP
			//Console.WriteLine("A_A");
			//OLD : ((?:\[SerializeField\])?)[\r\n\s]*?(?:\[Tooltip\(""([\S\s] *?)""\)\])[\r\n]*?(\t*?)(?:static\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:virtual\s)?(?:event\s)?([\w\d\<\>,]+?)\s(.+?);
			var matches = Regex.Matches(fileContents, @"([ \t]*?)(\[SerializeField\]\s+?)?\[Tooltip\(\""(.+?)\""\)\]\s+?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)");
			//Console.WriteLine("A_B");
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

				var extraction = new FieldExtraction
				{
					CommentType = CommentType.RegularComments,
					Serialized = match.Groups[2].Value.Contains("SerializeField"),
					CommentStuff = match.Groups[3].Value,
					Indentation = TabsToSpaces(match.Groups[1].Value),
					Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
					FieldType = match.Groups[5].Value,
					FieldName = match.Groups[6].Value
				};

				if (extraction.FieldType != "new" && !fields.Any(e => e.FieldName == extraction.FieldName))
				{
					//Console.WriteLine("A_Adding Field = " + extraction.FieldName);
					//Console.WriteLine("ADDING FIELD = " + extraction.FieldName);
					fields.Add(extraction);
				}
			}

			//

			//METHOD B : WITH XML
			//Console.WriteLine("A_C");
			//OLD : ([ \t]*?)((\/\/\/\s?.+?[\r\n]\s+?)+)(?:static\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:event\s)?(?:virtual\s)?([\w\d\<\>,]+?)\s([\w\d\<\>,]+?);
			//OLD 2 : ([ \t]*?)((\/\/\/\s?.+?[\r\n]\s+?)+)(?:static\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:event\s)?(?:virtual\s)?([\w\d\<\>,]+?)\s([\w\d\<\>,]+)
			//OLD 3 : ([ \t]*?)((\/\/\/\s?.+?[\r\n]\s+?)+)(?:static\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:event\s)?(?:virtual\s)?([\w\d\<\>,]+?)\s([\w\d\<\>,]+?);
			matches = Regex.Matches(fileContents, @"([ \t]*?)(\/\/\/\s+?<summary>(?:\s*?\/\/\/.+?[\r\n]+?)*)\s*?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|\b)*)\b\s*?([\w\d]+?(?:<.+?>)?(?:\[.*?\])?)\s([\d\w]+?)(?:;|\s?{|\s?\=\>)");
			//Console.WriteLine("A_D");

			//Console.WriteLine("MATCHES = " + matches.Count);
			for (int i = 0; i < matches.Count; i++)
			{
				//Console.WriteLine("MATCH = " + matches[i]);
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}
				/*if (match.Value.Contains("readonly"))
				{
					continue;
				}*/

				var extraction = new FieldExtraction
				{
					CommentType = CommentType.XMLComments,
					Serialized = false,
					CommentStuff = match.Groups[2].Value,
					Indentation = TabsToSpaces(match.Groups[1].Value),
					Publicity = match.Groups[3].Success ? match.Groups[3].Value : "",
					FieldType = match.Groups[4].Value,
					FieldName = match.Groups[5].Value
				};

				if (extraction.FieldType != "new" && extraction.FieldType != "public" && extraction.FieldType != "private" && extraction.FieldType != "protected" && extraction.FieldType != "static" && extraction.FieldType != "override" && extraction.FieldType != "virtual" && !fields.Any(e => e.FieldName == extraction.FieldName))
				{
					//Console.WriteLine("B_Adding Field = " + extraction.FieldName);
					fields.Add(extraction);
				}
			}

			//METHOD C : TOOLTIP INVERTED
			matches = Regex.Matches(fileContents, @"([ \t]*?)\[Tooltip\(\""(.+?)\""\)\]\s+?(\[SerializeField\]\s+?)?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)");
			//Console.WriteLine("A_B");
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

				var extraction = new FieldExtraction
				{
					CommentType = CommentType.RegularComments,
					Serialized = match.Groups[3].Value.Contains("SerializeField"),
					CommentStuff = match.Groups[2].Value,
					Indentation = TabsToSpaces(match.Groups[1].Value),
					Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
					FieldType = match.Groups[5].Value,
					FieldName = match.Groups[6].Value
				};

				if (extraction.FieldType != "new" && !fields.Any(e => e.FieldName == extraction.FieldName))
				{
					//Console.WriteLine("A_Adding Field = " + extraction.FieldName);
					//Console.WriteLine("ADDING FIELD = " + extraction.FieldName);
					fields.Add(extraction);
				}
			}

			//METHOD C : TOOLTIP INLINE
			matches = Regex.Matches(fileContents, @"([ \t]*?)(\[SerializeField,\s+?)?Tooltip\(\""(.+?)\""\)\]\s*?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly|abstract\s\s|\b)*)([\w\d]+?(?:\<.+?\>)?(?:\[.*?\])?)\s+([\w\d]+?)(?:;|\s+\=|\s*{)");
			//Console.WriteLine("A_B");
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

				var extraction = new FieldExtraction
				{
					CommentType = CommentType.RegularComments,
					Serialized = match.Groups[2].Value.Contains("SerializeField"),
					CommentStuff = match.Groups[3].Value,
					Indentation = TabsToSpaces(match.Groups[1].Value),
					Publicity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "",
					FieldType = match.Groups[5].Value,
					FieldName = match.Groups[6].Value
				};

				if (extraction.FieldType != "new" && !fields.Any(e => e.FieldName == extraction.FieldName))
				{
					//Console.WriteLine("A_Adding Field = " + extraction.FieldName);
					//Console.WriteLine("ADDING FIELD = " + extraction.FieldName);
					fields.Add(extraction);
				}
			}

			//METHOD D : REGULAR COMMENTS
			//Console.WriteLine("A_G");
			matches = Regex.Matches(fileContents, @"([ \t]*?)(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]+\s+?((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|\b)*)([\w\d]+?(?:\<.*?\>)?(?:\[.*?\])?)\s([\w\d\<\>,]+?)[\s;]");
			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];

				if (!match.Success)
				{
					continue;
				}

				//Console.WriteLine("Found Potential Field = " + match.Groups[4].Value);

				if (match.Value.Contains("readonly"))
				{
					continue;
				}

				var extraction = new FieldExtraction
				{
					CommentType = CommentType.RegularComments,
					Serialized = false,
					CommentStuff = match.Groups[2].Value,
					Indentation = TabsToSpaces(match.Groups[1].Value),
					Publicity = match.Groups[3].Success ? match.Groups[3].Value : "",
					FieldType = match.Groups[4].Value,
					FieldName = match.Groups[5].Value
				};

				if (extraction.FieldType != "new" && extraction.FieldType != "public" && extraction.FieldType != "private" && extraction.FieldType != "protected" && extraction.FieldType != "static" && extraction.FieldType != "override" && extraction.FieldType != "virtual" && extraction.FieldType != "class" && extraction.FieldType != "struct" && !fields.Any(e => e.FieldName == extraction.FieldName))
				{
					//Console.WriteLine("C_Adding Field = " + extraction.FieldName);
					fields.Add(extraction);
				}
			}

			return fields;
		}

		static List<MethodExtraction> ExtractMethods(string fileContents)
		{
			List<MethodExtraction> extractions = new List<MethodExtraction>();

			//METHOD A : XML COMMENTS

			//OLD 1 : ((\/\/\/\s?.+?[\r\n]\s+?)+)((?:static\s)?(public\s|private\s|protected\s|static\s)?(?:static\s)?(?:virtual\s)?([\w\d]+?)\s([\w\d\<\>,]+?)\(.*?\))[\r\n]*?\s*?\{
			var matches = Regex.Matches(fileContents, @"((\/\/\/\s?.+?[\r\n]\s+?)+)(?:\[.+?\]\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d]+?(?:\<.+?\>)?)\(.*?\))[\r\n]*?\s*?\{");

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}
				var extraction = new MethodExtraction
				{
					CommentStuff = match.Groups[1].Value,
					CommentType = CommentType.XMLComments,
					MethodHeader = match.Groups[3].Value,
					Visibility = match.Groups[4].Value,
					ReturnType = match.Groups[5].Value,
					MethodName = ValidatePath(match.Groups[6].Value)
				};

				if (extraction.ReturnType != "in" && extraction.ReturnType != "new" && extraction.ReturnType != "public" && extraction.ReturnType != "private" && extraction.ReturnType != "protected" && extraction.ReturnType != "static" && extraction.ReturnType != "override" && extraction.ReturnType != "virtual" && extraction.ReturnType != "class" && extraction.ReturnType != "struct" && !extractions.Any(e => e.MethodHeader == extraction.MethodHeader))
				{
					extractions.Add(extraction);
				}
			}

			//METHOD B : REGULAR COMMENTS

			//OLD 1 : (?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s+?((?:static\s)?(public\s|private\s|protected\s)?(?:static\s)?(?:virtual\s)?([\w\d]+?)\s([\w\d\<\>,]+?)\(.*?\))[\r\n]*?\s*?\{

			//NEW : 
			matches = Regex.Matches(fileContents, @"(?<!\/)(?:\/{2})+(?!\/)(.+?)[\r\n]\s+?(?:\[.+?\]\s*?)*(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d\<\>,]+?)\(.*?\))[\r\n]*?\s*?\{");

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}
				var extraction = new MethodExtraction
				{
					CommentStuff = match.Groups[1].Value,
					CommentType = CommentType.RegularComments,
					MethodHeader = match.Groups[2].Value,
					Visibility = match.Groups[3].Value,
					ReturnType = match.Groups[4].Value,
					MethodName = ValidatePath(match.Groups[5].Value),
				};

				if (extraction.ReturnType != "in" && extraction.ReturnType != "new" && extraction.ReturnType != "public" && extraction.ReturnType != "private" && extraction.ReturnType != "protected" && extraction.ReturnType != "static" && extraction.ReturnType != "override" && extraction.ReturnType != "virtual" && extraction.ReturnType != "class" && extraction.ReturnType != "struct" && !extractions.Any(e => e.MethodHeader == extraction.MethodHeader))
				{
					extractions.Add(extraction);
				}
			}


			//OLD 1 : ([ \t]*?)((?:static\s)?(public\s|private\s|protected\s)?(?:static\s)?(?:virtual\s)?([\w\d]+?)\s([\w\d\<\>,]+?)\(.*?\))[\r\n]*?\s*?\{


			//METHOD C : NO COMMENTS
			matches = Regex.Matches(fileContents, @"([ \t]*?)(((?:public\s|private\s|protected\s|static\s|event\s|virtual\s|sealed\s|readonly\s|abstract\s|virtual\s|\b)*)([\w\d]+?)\s([\w\d]+?(?:\<.+?\>)?)\(.*?\))[\r\n]*?\s*?\{");

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (!match.Success)
				{
					continue;
				}
				var extraction = new MethodExtraction
				{
					CommentStuff = "",
					CommentType = CommentType.NoComments,
					Indentation = TabsToSpaces(match.Groups[1].Value ?? ""),
					MethodHeader = match.Groups[2].Value,
					Visibility = match.Groups[3].Value,
					ReturnType = match.Groups[4].Value,
					MethodName = ValidatePath(match.Groups[5].Value)
				};

				if (extraction.ReturnType != "in" && extraction.ReturnType != "new" && extraction.ReturnType != "public" && extraction.ReturnType != "private" && extraction.ReturnType != "protected" && extraction.ReturnType != "static" && extraction.ReturnType != "override" && extraction.ReturnType != "virtual" && extraction.ReturnType != "class" && extraction.ReturnType != "struct" && !extractions.Any(e => e.MethodHeader == extraction.MethodHeader))
				{
					extractions.Add(extraction);
				}
				else
				{
					var existentElement = extractions.FirstOrDefault(e => e.MethodHeader == extraction.MethodHeader);
					if (existentElement != null)
					{
						existentElement.Indentation = extraction.Indentation;
					}
				}
			}










			return extractions;
		}

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
			//int commonIndentationLength = -1;
			int sizeOfList = -1;

			foreach (var i in commonIndentations)
			{
				//Console.WriteLine("Indentation of I" + i.Key + "I = " + i.Value.Count);
				//Console.WriteLine("A_ILENGTH = " + i.Key.Length);
			}

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

			//Console.WriteLine("FOUND I = I" + commonIndentation + "I");
			//Console.WriteLine("B_ILENGTH = " + commonIndentation.Length);
			return commonIndentation;
			/*if (commonIndentationLength < 0)
			{
				return null;
			}
			else
			{
				return commonIndentations[commonIndentationLength].First();
			}*/
		}
	}
}

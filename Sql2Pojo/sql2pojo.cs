/* *********************************************************************** */
//     Program: sql2pojo
//        Date: 03/08/2016
//      Author: Tim Stark
// 	 Copyright: Copyright (C) 2016  Tim Stark

// Description: This is a quick and simple program designed to
// 				convert a SQL file dump from PostgreSQL over 
//				to a set of Serialized Builder POJOs for each
//				database table.
//				
// 	   License: 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// 	You should have received a copy of the GNU General Public License
// 	along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
/* *********************************************************************** */
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Resources;


namespace Sql2Pojo
{
	class MainClass
	{

		private const string	TERMSNCONDITIONSFILE = "TermsAndConditions.txt";
		// use 4 spaces instead of hard tabs
		private const string 	SOFTTAB = "    ";
		// 0=errors and warnings only - no debug messages
		// 1=errors and warnings plus debug messages
		private const int 		ERRORWARNING=0;
		private const int		IMPORTANTINFO=1;
		private const int 		DEBUGMESSAGE=2;
		private const int		DebugLevel = 2; 
		private const char		SPACE = ((char)32);
		// HTAB is used by PostgreSQL 
		private const char		HTAB = ((char)09);

		public static void Main (string[] args)
		{
			string sqlFile = string.Empty;
			bool ignoreTableName = false;
			string PojoOutputPath = string.Empty;
			string PackageName= string.Empty;
			List<string> sqlFileItems = new List<string>();


			Write2Console("Sql2Pojo v0.0.1",IMPORTANTINFO);
			if (args == null) {
				DisplayInfo();
			}
			else {
//				Write2Console(string.Format("DEBUG: args length is {0}.",args.Length));
//				for (int i=0; i < args.Length; i++) {
//					Write2Console(string.Format("DEBUG: args[{0}] = {1}", i, args[i]));
//				}
				if (args.Length == 0) {
					DisplayInfo();
				}
				else {
					int i=0;
					foreach (string arg in args) {
						Write2Console(
							string.Format("DEBUG: Processing arg: [{0}]",arg),
							DEBUGMESSAGE);

						if (arg.ToLower() == "-i") {
							ignoreTableName=true;
						}
						else if (arg.ToLower() == "-h" || arg.ToLower() == "--h") {
							DisplayInfo();
							return;
						}
						else if (arg.ToLower() == "-w" || arg.ToLower() == "--w") {
							DisplayWarranty(true);
							return;
						}
						else if (arg.ToLower() == "-c" || arg.ToLower() == "--c") {
							DisplayTermsAndConditions();
							return;
						}
						else {
							if (i == 0) {
								sqlFile=arg;
							}
							else if (i == 1) {
								PojoOutputPath=arg;
							}
							else {
								PackageName=arg;
							}
							i++;
						}
					}	
				}
			}


			if ((sqlFileItems=LoadFileToStringArray(sqlFile)).Count == 0) {
				Write2Console(string.Format("Could not load file path/name [{0}]", sqlFile),
					IMPORTANTINFO);
				return;
			}

			CreatePOJOsFromSQLFile(
				sqlFileItems, 
				ignoreTableName, 
				PojoOutputPath,
				PackageName);
		}

		public static void Write2Console(string output, int debugLevel)
		{
			if (debugLevel <= DebugLevel) {
				Console.WriteLine(output);
			}
		}

		public static void DisplayInfo()
		{
			string info=string.Empty;

			info="sql2pojo converts a SQL file to POJO classes\n" +
				"Usage: sql2pojo [options] [sql file path and name] [pojo output path] [package name]\n\n" +
				"Options:\n" +
				"  -h || --h   This help text.\n" +
				"  -i   Ignore schema/db name.\n" +
				"  -w   Display license warranty information.\n" +
				"  -c   Display license terms and conditions\n\n";

			Write2Console(info, IMPORTANTINFO);
			DisplayWarranty(false);
		}

		public static void DisplayWarranty(bool fullinfo) {
			string info=
				"sql2pojo Copyright (C) 2016  Tim Stark\n\n" +
				"This program comes with ABSOLUTELY NO WARRANTY;\n" +
				"for details type `sql2pojo -w'.\n" +
				"This is free software, and you are welcome to redistribute it\n" +
				"under certain conditions; type `sql2pojo -c' for details.\n\n";

			if (fullinfo) {
				info = info +
					"This program is free software: you can redistribute it and/or modify\n" +
					"it under the terms of the GNU General Public License as published by\n" +
					"the Free Software Foundation, either version 3 of the License, or\n" +
					"(at your option) any later version.\n\n" +
					"This program is distributed in the hope that it will be useful,\n" +
					"but WITHOUT ANY WARRANTY; without even the implied warranty of\n" +
					"MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n" +
					"GNU General Public License for more details.\n\n" +
					"You should have received a copy of the GNU General Public License\n" +
					"along with this program.  If not, see <http://www.gnu.org/licenses/>.\n";
			}

			Write2Console(info, IMPORTANTINFO);
		}

		public static void DisplayTermsAndConditions()
		{
			//string codebase=System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
			//string filespec=System.IO.Path.GetDirectoryName(codebase);

			//filespec=filespec + "/../../" + TERMSNCONDITIONSFILE;
			string filespec=AppDomain.CurrentDomain.BaseDirectory;
			filespec=System.IO.Directory.GetParent(filespec).ToString();
			filespec=System.IO.Directory.GetParent(filespec).ToString();
			filespec=System.IO.Directory.GetParent(filespec).ToString();
			filespec=System.IO.Path.Combine(filespec,TERMSNCONDITIONSFILE);
			Console.WriteLine("FileSpec:" + filespec);
			if (System.IO.File.Exists(filespec)) {
				System.IO.StreamReader loadedFile =
					new System.IO.StreamReader(filespec);
				string terms=loadedFile.ReadToEnd();
				Write2Console(terms, IMPORTANTINFO);
			}	
			else {
				Write2Console("Terms and Conditions File could not be loaded!\n", ERRORWARNING);
			}
		}

		public static string GetDataTypeFromSQL(string str) {
			if (str.Contains("varchar")) {
				return "String";
			}
			else if (str.Contains("BIGSERIAL")) {
				return "Long";
			}
			else if (str.Contains("char")) {
				return "Char";
			}
			else if (str.Contains("timestamp")) {
				return "Datetime";
			}
			else if (str.Contains("date")) {
				return "Date";
			}
			else if (str.Contains("int4")) {
				return "Int";
			}
			else {
				return("unknown");
			}
		}

		private static List<string> LoadFileToStringArray(string filespecification)		{
			System.IO.StreamReader loadedFile=new System.IO.StreamReader(filespecification);

			return loadedFile.ReadToEnd().Split('\n').ToList();
		}

		private static void CreatePOJOsFromSQLFile(
			List<string> items,
			bool ignoreTableNames,
			string pojoOuputPath,
			string packageName) 
		{
			List<string>rawSqlData=new List<string>();
			bool foundTable=false;

			Write2Console(
				string.Format("DEBUG: CreatePOJOsFromSQLFile - items:{0} ignore:{1}",
					items.Count, ignoreTableNames),
				DEBUGMESSAGE);

			foreach (string item in items) {
				// find the CREATE TABLE and then ingest it
				if (item.ToLower().StartsWith("create table")) {
					Write2Console("DEBUG: Found CREATE TABLE", DEBUGMESSAGE);
					rawSqlData.Clear();
					if (ignoreTableNames) {
						if (item.Contains(".")) {
							rawSqlData.Add(item.Split('.')[1]);
						}
						else {
							rawSqlData.Add(item);
						}
					}
					else {
						rawSqlData.Add(item);
					}
					foundTable=true;
				}
				else if (foundTable) {
					if (item.Trim() == ")") {
						rawSqlData.Add(item);
						foundTable=false;

						if (DebugLevel >= DEBUGMESSAGE) {
							Write2Console(
								string.Format("DEBUG: Pre-classes.Add() RawSqlData Count [{0}]\n",
									rawSqlData.Count), DEBUGMESSAGE);

							foreach (string s in rawSqlData) {
								Write2Console(s, DEBUGMESSAGE);
							}
						}
						CreatePOJOFromList(
							rawSqlData,
							pojoOuputPath,
							packageName);
					}
					else {
						bool ignoreLine=
							item.Contains("PRIMARY KEY") ||
							item.Contains("CONSTRAINT ") ||
							item.Contains("NOT DEFERRABLE") ||
							item.Contains("ON DELETE") ||
							item.Contains("ON UPDATE") ||
							item.Contains("MATCH SIMPLE") ||
							item.Contains("REFERENCES ");

						if (!ignoreLine) {
							rawSqlData.Add(item);
						}
					}
				}
			}
		}

		private static void CreatePOJOFromList(
			List<string> RawSqlData, 
			string pojoOutputPath,
			string packageName) 
		{
			List<string> classFile=new List<string>();

			string className=string.Empty;
			List<string> columns=new List<string>();
			List<string> dataTypes=new List<string>();

			classFile.Add(string.Format("{0}\n\n",packageName));
			foreach (string rawLine in RawSqlData) {
				string tmpLine=rawLine.Replace(HTAB, SPACE).Trim();
				string[] temp=tmpLine.Split(SPACE);
				Write2Console(string.Format("DEBUG LINE:[{0}]",tmpLine),DEBUGMESSAGE);
				Write2Console(string.Format("DEBUG ARRAY SIZE:{0}",
					temp.Length),DEBUGMESSAGE);
				Write2Console("DEBUG:[" + temp[0] + "]", DEBUGMESSAGE);

				if (tmpLine.Contains(" (")) {
					Write2Console(string.Format("Class Name:{0}",temp),DEBUGMESSAGE);
					className=char.ToUpper(temp[0][0]) + temp[0].Substring(1);
				}
				else if (temp[0] == ")") {
					Write2Console("End of Class",DEBUGMESSAGE);
				}
				else { // this thing is a column
					string column=temp[0].Trim();
					string datatype=GetDataTypeFromSQL(temp[1]).Trim();
					Write2Console(
						string.Format("Column:{0} DataType:{1}", column, datatype),
						DEBUGMESSAGE);
					columns.Add(column);
					dataTypes.Add(datatype);
				}
			}

			// now write output
			classFile.Add(
				string.Format("public class {0} implements Serializable {{\n",
					className));
			classFile.Add(
				string.Format("{0}private static final long serialVersionUID = 1L;\n\n",
					SOFTTAB));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add(
					string.Format("{0}private final {1} {2};\n", 
						SOFTTAB,
						dataTypes[i], 
						columns[i]));
			}

			classFile.Add("\n");

			// constructor
			classFile.Add(
				string.Format("{0}public {1} () {{\n", 
					SOFTTAB,
					className));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add(
					string.Format("{0}{0}{1} = null;\n", 
						SOFTTAB,
						columns[i]));
			}
			classFile.Add(
				string.Format("{0}}}\n\n",
					SOFTTAB));

			// setters
			classFile.Add(
				string.Format("{0}@SuppressWarnings(\"rawtypes\")\n",
					SOFTTAB));
			classFile.Add(
				string.Format("{0}public static class Builder<T extends Builder> {{\n",
					SOFTTAB));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add(
					string.Format("{0}{0}private {1} {2};\n",
						SOFTTAB,
						dataTypes[i], 
						columns[i]));
			}

			classFile.Add(
				string.Format("\n{0}{0}public Builder() {{}}\n\n",
					SOFTTAB));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add( 
					string.Format("{0}{0}public T {1}({1} {2}) {{\n", 
						SOFTTAB,
						columns[i], 
						dataTypes[i], 
						columns[i]));
				classFile.Add(
					string.Format("{0}{0}{0}this.{1} = {2};\n", 
						SOFTTAB,
						columns[i], 
						columns[i]));
				classFile.Add(
					string.Format("{0}{0}{0}return (T) this;\n{0}{0}}}\n\n",
						SOFTTAB));
			}
			classFile.Add(
				string.Format("{0}}}\n\n",
					SOFTTAB));

			// getters
			classFile.Add(
				string.Format("{0}@SuppressWarnings(\"rawtypes\")\n",
					SOFTTAB));
			classFile.Add(
				string.Format("{0}protected {1} Builder<T extends Builder> {{\n",
					SOFTTAB,
					className));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add(
					string.Format("{0}{0}this.{1} = builder.{1};\n",
						SOFTTAB,
						columns[i]));
			}

			classFile.Add(string.Format("{0}}}\n\n",SOFTTAB));

			for (int i=0; i < columns.Count; i++) {
				classFile.Add( 
					string.Format("{0}public {1} get{2}() {{\n", 
						SOFTTAB,
						dataTypes[i], 
						char.ToUpper(columns[i][0])+columns[i].Substring(1)));
				classFile.Add(
					string.Format("{0}{0}return {1};\n", 
						SOFTTAB,
						columns[i]));
				classFile.Add(
					string.Format("{0}{0}return (T) this;\n{0}}}\n\n",
						SOFTTAB));
			}

			classFile.Add(
				string.Format("{0}@Override\n",
					SOFTTAB));
			classFile.Add(
				string.Format("{0}public String toString() {{\n",
					SOFTTAB));
			classFile.Add(
				string.Format("{0}{0}return ToStringBuilder.reflectionToString(this);\n",
					SOFTTAB));
			classFile.Add(
				string.Format("{0}}}\n",
					SOFTTAB));
			classFile.Add("}\n");

			string outFileName=
				pojoOutputPath +
				string.Format("{0}.java",className);
			// delete existing file if exists
			if (System.IO.File.Exists(outFileName)) {
				System.IO.File.Delete(outFileName);
			}
			// dump the classFile to new class
			System.IO.StreamWriter outFile=new System.IO.StreamWriter(outFileName);

			string classBuffer=string.Join("", classFile.ToArray());
			outFile.Write(classBuffer);
			outFile.Close();
		}
	}
}

using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace Sql2Pojo
{
	class MainClass
	{

		private const string 	TAB = "    ";
		private const bool 		DebugMode = true;


		public static void Main (string[] args)
		{
			string sqlFile = string.Empty;
			bool ignoreTableName = false;
			List<string> sqlFileItems = new List<string>();

			Console.WriteLine ("Sql2Pojo v0.0.1");
			if (args == null) {
				DisplayInfo();
			}
			else {
				if (DebugMode) {
					Console.WriteLine(string.Format("DEBUG: args length is {0}.",args.Length));
					for (int i=0; i < args.Length; i++) {
						Console.WriteLine(string.Format("DEBUG: args[{0}] = {1}", i, args[i]));
					}
				}
				if (args.Length == 0) {
					DisplayInfo();
				}
				else {
					foreach (string arg in args) {
						if (DebugMode) {
							Console.WriteLine(string.Format("DEBUG: Processing arg: [{0}]",arg));

						}


						if (arg.ToLower() == "-i") {
							ignoreTableName=true;
						}
						else if (arg.ToLower() == "-h" || arg.ToLower() == "--h") {
							DisplayInfo();
						}
						else {
							// assume file specification
							sqlFile=arg;
						}
					}	
				}
			}


			if ((sqlFileItems=LoadFileToStringArray(sqlFile)).Count == 0) {
				Console.WriteLine(string.Format("Could not load file path/name [{0}]", sqlFile));
				return;
			}

			CreatePOJOsFromSQLFile(sqlFileItems, ignoreTableName);

			//CreateMongoDBDataFromCSVs();
			//CreatePOJOFromSQL();
		}

		public static void DisplayInfo()
		{
			string info=string.Empty;

			info="sql2pojo converts a SQL file to POJO classes\n" +
				"Usage: sql2pojo [options] [sql file path and name]\n\n" +
				"Options:\n" +
				"  -h   This help text.\n" +
				"  -i   Ignore table name.\n";

			Console.WriteLine(info);
		}
	

		public static string RemoveSpecialCharacters(string str) {
			StringBuilder sb = new StringBuilder();
			foreach (char c in str) {
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_') {
					sb.Append(c);
				}
			}
			return sb.ToString();
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
			bool ignoreTableNames) 
		{
			List<string>classes=new List<string>();
			string temp=string.Empty;

			bool foundTable=false;

			foreach (string item in items) {
				// find the CREATE TABLE and then ingest it
				if (item.ToLower().StartsWith("create table")) {
					temp=string.Empty;
					if (!ignoreTableNames) {
						if (item.Contains(".")) {
							temp=item.Split('.')[1];
						}
						else {
							temp=item;
						}
					}
					foundTable=true;
				}
				else if (foundTable) {
					temp=temp+item;
					if (item.Trim() == ")") {
						classes.Add(temp);
						foundTable=false;
					}
				}
			}

			foreach (string c in classes) {
				Console.WriteLine(c);
			}

		}

		private static void CreatePOJOFromSQL() {
			string sqlInputFile="/Users/tim/Desktop/por_person.sql";
			string line=string.Empty;
			int lineCount=0;

			string packageName="gov.nih.nhlbi.por.model;";

			string classFile=string.Empty;

			System.IO.StreamReader sqlFile=new System.IO.StreamReader(sqlInputFile);

			string className=string.Empty;
			List<string> columns=new List<string>();
			List<string> dataTypes=new List<string>();

			classFile=string.Format("{0}\n\n",packageName);
			while ((line=sqlFile.ReadLine()) != null)
			{
				string[] temp=line.Split(' ');
				if (line.Contains(" (")) {
					Console.WriteLine(string.Format("Class Name:{0}",temp));
					className=char.ToUpper(temp[0][0]) + temp[0].Substring(1);
				}
				else if (temp[0] == ")") {
					Console.WriteLine("End of Class");
				}
				else { // this thing is a column
					string column=temp[0].Trim();
					string datatype=GetDataTypeFromSQL(temp[1]).Trim();
					Console.WriteLine("Column:{0} DataType:{1}", column, datatype);
					columns.Add(column);
					dataTypes.Add(datatype);
				}

				lineCount++;
			}
			sqlFile.Close();

			// now write output

			classFile=classFile+string.Format("public class {0} implements Serializable {{\n",className);
			classFile=classFile+string.Format("{0}private static final long serialVersionUID = 1L;\n\n",
				TAB);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile+string.Format("{0}private final {1} {2};\n", 
					TAB,
					dataTypes[i], 
					columns[i]);
			}

			classFile=classFile+"\n";

			// constructor
			classFile=classFile+string.Format("{0}public {1} () {{\n", 
				TAB,
				className);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile + string.Format("{0}{0}{1} = null;\n", 
					TAB,
					columns[i]);
			}
			classFile=classFile+string.Format("{0}}}\n\n",
				TAB);


			// setters
			classFile=classFile + string.Format("{0}@SuppressWarnings(\"rawtypes\")\n",
				TAB) +
				string.Format("{0}public static class Builder<T extends Builder> {{\n",
					TAB);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile + string.Format("{0}{0}private {1} {2};\n",
					TAB,
					dataTypes[i], 
					columns[i]);
			}

			classFile=classFile + string.Format("\n{0}{0}public Builder() {{}}\n\n",
				TAB);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile + 
					string.Format("{0}{0}public T {1}({1} {2}) {{\n", 
						TAB,
						columns[i], 
						dataTypes[i], 
						columns[i]) +
					string.Format("{0}{0}{0}this.{1} = {2};\n", 
						TAB,
						columns[i], 
						columns[i]) +
					string.Format("{0}{0}{0}return (T) this;\n{0}{0}}}\n\n",
						TAB);
			}
			classFile=classFile+string.Format("{0}}}\n\n",
				TAB);
			
			// getters
			classFile=classFile + string.Format("{0}@SuppressWarnings(\"rawtypes\")\n",
				TAB) +
				string.Format("{0}protected {1} Builder<T extends Builder> {{\n",
					TAB,
					className);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile + string.Format("{0}{0}this.{1} = builder.{2};\n",
					TAB,
					columns[i],
					columns[i]);
			}

			classFile=classFile + string.Format("{0}}}\n\n",
				TAB);

			for (int i=0; i < columns.Count; i++) {
				classFile=classFile + 
					string.Format("{0}public {1} get{2}() {{\n", 
						TAB,
						dataTypes[i], 
						char.ToUpper(columns[i][0])+columns[i].Substring(1)) +
					string.Format("{0}{0}return {1};\n", 
						TAB,
						columns[i]) +
					string.Format("{0}{0}return (T) this;\n{0}}}\n\n",
						TAB);
			}

			classFile=classFile + string.Format("{0}@Override\n",
				TAB) +
				string.Format("{0}public String toString() {{\n",
					TAB) +
				string.Format("{0}{0}return ToStringBuilder.reflectionToString(this);\n",
					TAB) +
				string.Format("{0}}}\n",
					TAB) +
				"}\n";


			// dump the classFile to new class
			System.IO.StreamWriter outFile=
				new System.IO.StreamWriter(string.Format("/Users/tim/Desktop/{0}.java",className));
			outFile.Write(classFile);
			outFile.Close();



		}

		private static void CreateMongoDBDataFromCSVs() {
			// TODO: parse args and allow passing of coninfo and scope parameters - e.g. only a single table
			// Defaults 
			// 	server is 192.168.1.205/redhawk
			// 

			bool ignoreFirstLine=true;
			bool removeSpecialCharacters=true;

			List<string> csvInputFiles=new List<string>();
			List<string> JSONOutputFiles=new List<string>();

			csvInputFiles.Add("/Users/tim/Desktop/gm_namesFirst.csv");
			csvInputFiles.Add("/Users/tim/Desktop/gm_namesLast.csv");

			JSONOutputFiles.Add("/Users/tim/Desktop/JSONFirstName.txt");
			JSONOutputFiles.Add("/Users/tim/Desktop/JSONLastName.txt");

			List<string> mongoData=new List<string>();

			//foreach (string s in csvInputFiles) {
			for (int fileIndex=0; fileIndex < csvInputFiles.Count; fileIndex++) {
				string line=string.Empty;
				int lineCount=0;

				mongoData.Clear();
				System.IO.StreamReader csvFile=new System.IO.StreamReader(csvInputFiles[fileIndex]);
				while ((line=csvFile.ReadLine()) != null)
				{
					if (ignoreFirstLine && lineCount < 1) {
						Console.WriteLine("skipping first line");
					}
					else {
						string tmpData=string.Empty;
						if (removeSpecialCharacters) {
							tmpData=RemoveSpecialCharacters(line);
						}
						else {
							tmpData=line;
						}
						Console.Write(tmpData);
						mongoData.Add(tmpData);
					}
					lineCount++;
				}
				csvFile.Close();

				// dump the data out as a JSON array to the corresponding output file
				System.IO.StreamWriter outFile=new System.IO.StreamWriter(JSONOutputFiles[fileIndex]);
				outFile.Write("{\t[");
				int i=0;
				int cnt=mongoData.Count;
				foreach (string s in mongoData) {
					string tmpData=string.Empty;
					tmpData=tmpData + string.Format("\"{0}\"",s);
					if (i < cnt-1) {	
						tmpData=tmpData + ",";
					}
					outFile.Write(tmpData);

					i++;
				}
				outFile.Write("]\n}\n");
				outFile.Close();

			}
			Console.ReadLine();

		}

		private static void CreateMongoDBArraysFromDB() {

			string server="redhawk";
			string userid="tim";
			string password="password1";
			string db="gmDev";
			string port="3306";
			string pooling="false";
			List<string> tables=new List<string>();

			string coninfo = string.Format("Server={0};User ID={1};Password={2};Database={3};Port={4};Pooling={5}",
				server, userid, password, db, port, pooling);

			// TODO: integrate into main logic loop, keep now for reference, delete when final
			//			try
			//			{
			//				Console.WriteLine( "Trying to open database connection ..." );
			//				connection.Open();
			//			}
			//			catch(Exception ex)
			//			{
			//				Console.WriteLine(ex.ToString());
			//			}

			List<string> classFiles=new List<string>();

			DataTable schema = null;
			using (MySqlConnection con = new MySql.Data.MySqlClient.MySqlConnection(coninfo))
			{
				foreach (string tableName in tables) {
					string selectInfo=string.Format("SELECT * FROM {0}.{1}",db, tableName);
					using (MySqlCommand schemaCommand = new MySql.Data.MySqlClient.MySqlCommand(selectInfo, con))
					{
						con.Open();
						using (MySqlDataReader reader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly))
						{
							schema = reader.GetSchemaTable();
						}

						// set up new class file string
						string newClass=string.Empty;

						newClass=newClass+
							string.Format("\tpublic class {0}\n",tableName)+
							"\t{\n";

						Console.WriteLine("Schema Rows Count:" + schema.Rows.Count);
						string[,] columnInfo=new string[schema.Rows.Count,2];
						int c=0;

						// create private variables
						foreach (DataRow col in schema.Rows)
						{
							columnInfo[c,0]=ConvertTypeToMySqlType(col["DataType"].ToString());
							columnInfo[c,1]=col["ColumnName"].ToString();

							// create private variable
							newClass=newClass + 
								string.Format("\t\tprivate {0} _{1}\n", 
									columnInfo[c,0], 
									columnInfo[c,1]);


							Console.WriteLine("\tREADING: {0}: {1}: {2}: {3}: {4}: {5}", 
								col["ColumnName"].ToString(), 
								col["DataType"].ToString(),
								col["ColumnSize"].ToString(),
								col["ProviderType"].ToString(),
								col["BaseColumnName"].ToString(),
								col["NumericScale"].ToString());

							c++;

						}
						newClass=newClass+"\n";

						// create getters and setters
						for (int i=0; i < schema.Rows.Count; i++) {

							newClass=newClass + 
								string.Format("\t\tpublic {0} {1}\n", 
									columnInfo[i,0], 
									PublicVariableName(columnInfo[i,1]));

							newClass=newClass + "\t\t{\n" +
								"\t\t\tget {\n" +
								"\t\t\t\treturn " + string.Format("_{0};\n",columnInfo[i,1]) +
								"\t\t\t}\n" +
								"\t\t\tset {\n" +
								"\t\t\t\t" + string.Format("_{0}",columnInfo[i,1]) +
								" = value;\n" +
								"\t\t\t}\n";

							newClass=newClass + "\t\t}\n";
						}


						// create default constructor(s)
						string newClassName=tableName;
						newClass=newClass + "\n" + string.Format("\t\tpublic {0} (", newClassName);

						for (int i=0; i < schema.Rows.Count; i++) {
							if (i > 0) {
								newClass=newClass+",";
							}
							newClass=newClass + 
								string.Format("{0} {1}", columnInfo[i,0],columnInfo[i,1]);
						}

						newClass=newClass + ")\n"+
							"\t\t{\n";
						newClass=newClass + "\t\t}\n";

						// TODO: add more default constructors as needed

						// close class
						newClass=newClass + "\t}\n";

						// close namespace
						newClass=newClass + "}\n";

						classFiles.Add(newClass);
						con.Close();
					}
				}
			}
			foreach (string c in classFiles) {
				Console.WriteLine(c + "\n---\n");
			}
		}

		private static string ConvertTypeToMySqlType(string systemType)
		{
			string retType;
			if (systemType == "System.Int32") {
				retType="int";
			}
			else if (systemType == "System.Int64") {
				retType="long";
			}
			else if (systemType == "System.DateTime") {
				retType="DateTime";
			}
			else if (systemType == "System.Single") {
				retType="float";
			}
			else if (systemType == "System.String") {
				retType="string";
			}
			else {
				retType=systemType;
			}
			return retType;
		}

		private static string PublicVariableName(string columnName)
		{
			if (columnName == null) return "";
			return char.ToUpper(columnName[0]) + columnName.Substring(1);

		}
	}
}

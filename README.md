# SQL2Pojo
This is a quick and simple program designed to convert a SQL file dump from PostgreSQL over to a set of Serialized Builder POJOs for each database table.

Note: This is my first GitHub project.  I am publishing this project because I thought others might find use in it and/or be willing to contribute towards improving it further.  For now this does all I need it to do, but as I use it, I'm sure I will come up with additional wish list items and continue to grow the program as they are added in.

Program Usage:

sql2pojo [options] [input SQL path/file] [pojo output path] [java package name]

ToDo:

1. Need to improve args parsing to be much more robust, very kludgy at the moment

2. Need to go back and remove most of the debug code but for now you can set DebugLevel to 0 or 1 to avoid seeing it.

3. Need to support Windows - now this is primarily using Linux/Unix path and file strings

4. Need to add support for MySQL and/or other versions or variations of SQL files

5. Need to add support for additional data types within PostgreSQL as well as others

6. Need to add more robust support for keyword parsing of SQL files for items removed e.g. ON DELETE, NOT DEFERRED

7. Nice to have would be to break the project up into multiple files for better manageability

8. Could change output file from a list to a string using ConCat, but this seemed a bit easier to manage with List<string>

9. Could also add better, more robust support for other character sets or localization for other languages besides english

10. Could add optionally enable the Serialization/Builder vs. having it be the current default and only POJO pattern

11. Need to add in a imports/libraries section

12. Need to add option for adding decorators as well

13. Would be nice if this could select output lanaguge to support POCO/C# or other programming languages besides just POJO/Java e.g. Python, Ruby, VB



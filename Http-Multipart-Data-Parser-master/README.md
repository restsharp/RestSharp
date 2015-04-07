What is it?
===========

The Http Multipart Parser does it exactly what it claims on the tin: parses multipart/form-data. This particular
parser is well suited to parsing large data from streams as it doesn't attempt to read the entire stream at once and
procudes a set of streams for file data.

Installation
=============
Simply add the HttpMultipartParser project to your solution and reference it in the projects you want to use it in.

Documentation
=============
Additional documentation is available from the HttpMultipartParserDocumentation project and is compiled from the
XML documentation.

Dependencies
============
The parser was built and tested for NET 4.0. Versions lower then this may work but are untested.

Documentation is generated from XML comments using sandcastle. The HttpMultipartParserDocumentation project
requires the [sandcastle help file builder](http://shfb.codeplex.com/) plugin.

How do I use it?
================
1. Create a new MultipartFormDataParser with the stream containing the multipart/form-data.
2. Access the data through the parser.

Examples
========

Single file
-----------

	// stream:
	-----------------------------41952539122868
	Content-Disposition: form-data; name="username"

	example
	-----------------------------41952539122868
	Content-Disposition: form-data; name="email"

	example@data.com
	-----------------------------41952539122868
	Content-Disposition: form-data; name="files[]"; filename="photo1.jpg"
	Content-Type: image/jpeg

	ExampleBinaryData012031203
	-----------------------------41952539122868--

	// parse:
    var parser = new MultipartFormDataParser(stream);

	// From this point the data is parsed, we can retrieve the
	// form data from the Parameters dictionary:
	var username = parser.Parameters["username"].Data;
	var email = parser.Parameters["email"].Data;

	// Files are stored in a list:
	var file = parser.Files.First();
	string filename = file.FileName;
	Stream data = file.Data;

Multiple Files
-----------

	// stream:
	-----------------------------41111539122868
	Content-Disposition: form-data; name="files[]"; filename="photo1.jpg"
	Content-Type: image/jpeg

	MoreBinaryData
	-----------------------------41111539122868
	Content-Disposition: form-data; name="files[]"; filename="photo2.jpg"
	Content-Type: image/jpeg

	ImagineLotsOfBinaryData
	-----------------------------41111539122868--

	// parse:
    var parser = new MultipartFormDataParser(stream);

	// Loop through all the files
	foreach(var file in parser.Files)
	{
	    Stream data = file.Data;

		// Do stuff with the data.
	}

Licensing
=========
Please see LICENSE
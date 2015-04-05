Copied dlls from https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki

VERSION: System.Data.SQLite 1.0.96.0 (3.8.8.3)
The Visual C++ 2013 Update 2 runtime for x64 and the .NET Framework 4.5.1 are required

Precompiled Binaries for 64-bit Windows (.NET Framework 4.5.1)
Precompiled Binaries for 32-bit Windows (.NET Framework 4.5.1)

sqlite-netFx451-binary-x64-2013-1.0.96.0
sqlite-netFx451-binary-Win32-2013-1.0.96.0

Application deployment should looks something like this:	
	<bin>\App.exe (optional, managed-only application executable assembly)
	<bin>\App.dll (optional, managed-only application library assembly)
	<bin>\System.Data.SQLite.dll (required, managed-only core assembly)
	<bin>\System.Data.SQLite.Linq.dll (optional, managed-only LINQ assembly)
	<bin>\System.Data.SQLite.EF6.dll (optional, managed-only EF6 assembly)
	<bin>\x86\SQLite.Interop.dll (required, x86 native interop assembly)
	<bin>\x64\SQLite.Interop.dll (required, x64 native interop assembly)

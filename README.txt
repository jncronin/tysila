Tysila
------

A CIL to native code compiler designed for building and as the JIT
compiler for tysos (https://github.com/jncronin/tysos)

Building
--------

Run 'msbuild' in the repository root

For dotnet on linux/windows:

dotnet publish -c Release -p:TargetLatestRuntimePatch=true -p:PublishDir=bin/Release -r <RID>

where RID is selected from
https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
 (e.g. linux-x64 or win10-x64)
 
Add the resultant 'publish' path for tysila4 (e.g. tysila4/bin/Release) to PATH)


Usage
-----

tysila4 -t arch [-L repository_path] [-i] [-o output_file] input_file

Currently, only arch=x86_64 is fully supported, with limited support for x86 and experimental arm support.


tysila expects mscorlib from coreclr (https://github.com/dotnet/coreclr) to
be available.  Please specify the location with the -L option.  Pre-built
copies of CoreCLR 2.0 (and parts of CoreFX) are available from 
https://www.tysos.org/files/tools/coreclr-2.0.0.zip


libsupcs
--------

All native code files produced by tysila are expected to be linked with 
libsupcs (also available in the repository).  This contains various runtime
functions related to Reflection, string support and architecture-specific
extensions.

libsupcs can be compiled using the provided libsupcs.tmk tymake file.
This requires a functioning tymake (https://github.com/jncronin/tymake)
as well as cross compilers for the appropriate architecture and (for x86_64)
yasm.

For Windows hosts, the appropriate tools (excluding tymake) can be obtained
from https://www.tysos.org/files/tools/crossxwin-7.3.0.zip
For Linux, please obtain yasm from your distribution and follow the
instructions at https://wiki.osdev.org/GCC_Cross-Compiler to build appropriate
cross compilers for the 'x86_64-elf' target.


To build:

path_to_tymake/tymake.exe "libsupcs.tmk"

and answer the prompts with the appropriate paths (alternatively add all
executables to the PATH environment variable).

This buils script expects tysila4 to be installed under ./tysila4/bin/Release
(default if the above dotnet publish command is used).  If not, please edit
libsupcs.tmk appropriately to set the TYSILA and GENMISSING variables, or set
them as environment variables prior to running.


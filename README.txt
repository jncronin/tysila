Tysila
------

A CIL to native code compiler designed for building and as the JIT
compiler for tysos (https://github.com/jncronin/tysos)

Building
--------

Run 'msbuild' in the repository root


Usage
-----

tysila4 -t arch [-L repository_path] [-i] [-o output_file] input_file

Currently, only arch=x86_64 is supported.


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
executables to the PATH environment variable)

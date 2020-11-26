#!/bin/bash
set -e

# Based on https://wiki.osdev.org/GCC_Cross-Compiler

# install prerequisites
DEBIAN_FRONTEND='noninteractive' apt-get update
DEBIAN_FRONTEND='noninteractive' apt-get install -y \
  curl nasm build-essential bison flex libgmp3-dev \
  libmpc-dev libmpfr-dev texinfo yasm libunwind8 libunwind-dev \
  unzip mono-mcs

# download and extract sources
mkdir ~/src && cd ~/src
curl -s https://ftp.gnu.org/gnu/binutils/binutils-2.30.tar.gz \
  --output binutils-2.30.tar.gz > /dev/null
curl -s https://ftp.gnu.org/gnu/gcc/gcc-8.1.0/gcc-8.1.0.tar.gz \
  --output gcc-8.1.0.tar.gz > /dev/null
tar -xf binutils-2.30.tar.gz
tar -xf gcc-8.1.0.tar.gz

# export variables
export PREFIX="/usr/local/cross"
export TARGET=i686-elf
export PATH="$PREFIX/bin:$PATH"

# build binutils 
cd ~/src

mkdir build-binutils
cd build-binutils
../binutils-2.30/configure --target=$TARGET --prefix="$PREFIX" \
  --with-sysroot --disable-nls --disable-werror
make
make install

# build gcc
cd ~/src
 
# The $PREFIX/bin dir _must_ be in the PATH.
which -- $TARGET-as || echo $TARGET-as is not in the PATH
 
mkdir build-gcc
cd build-gcc
../gcc-8.1.0/configure --target=$TARGET --prefix="$PREFIX" --disable-nls \
  --enable-languages=c,c++ --without-headers
make -j$((`nproc`+1)) all-gcc
make -j$((`nproc`+1)) all-target-libgcc
make install-gcc
make install-target-libgcc
cd ..

# and again for 64 bit versions
export TARGET=x86_64-elf

mkdir build-binutils64
cd build-binutils64
../binutils-2.30/configure --target=$TARGET --prefix="$PREFIX" \
  --with-sysroot --disable-nls --disable-werror
make
make install

# build gcc
cd ~/src
 
# The $PREFIX/bin dir _must_ be in the PATH.
which -- $TARGET-as || echo $TARGET-as is not in the PATH
 
mkdir build-gcc64
cd build-gcc64
../gcc-8.1.0/configure --target=$TARGET --prefix="$PREFIX" --disable-nls \
  --enable-languages=c,c++ --without-headers
make -j$((`nproc`+1)) all-gcc
make -j$((`nproc`+1)) all-target-libgcc
make install-gcc
make install-target-libgcc
cd ..

# get tymake and tysila
cd ~/src
git clone https://github.com/jncronin/tymake.git
git clone --recurse https://github.com/jncronin/tysila.git

# build them
cd ~/src/tymake
dotnet publish -c Release -p:TargetLatestRuntimePatch=true -p:PublishDir=/usr/local/tymake -r linux-x64
export PATH="/usr/local/tymake:$PATH"

cd ~/src/tysila
dotnet publish -c Release -p:TargetLatestRuntimePatch=true -p:PublishDir=/usr/local/tysila -r linux-x64
export PATH="/usr/local/tysila:$PATH"

# get coreclr 2.0 prebuilt
cd ~/src
wget --no-check-certificate https://31.172.250.186/files/tools/coreclr-2.0.0.zip
unzip coreclr-2.0.0.zip
cp -dpR coreclr /usr/local
export PATH="/usr/local/coreclr:$PATH"

# build libsupcs
cd ~/src/tysila
mkdir -p tysila4/bin/Release
mkdir -p /usr/local/libsupcs
dotnet build -c Release libsupcs
cp tysila4/bin/Release/netcoreapp2.0/libsupcs.dll tysila4/bin/Release/netcoreapp2.0/metadata.dll tysila4/bin/Release
tymake "TYSILA=\"/usr/local/tysila/tysila4\";GENMISSING=\"/usr/local/tysila/genmissing\";MSCORLIB=\"/usr/local/coreclr/mscorlib.dll\";INSTALL_DIR=\"/usr/local/libsupcs\";" libsupcs.tmk
export PATH="/usr/local/libsupcs:$PATH"

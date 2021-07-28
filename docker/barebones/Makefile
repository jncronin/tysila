all: barebones.iso

loader.o: loader.asm
	nasm -felf -o loader.o loader.asm

netstandard2.0/barebones.dll: kernel.cs
	dotnet build

kernel.o: netstandard2.0/barebones.dll
	dotnet run --project ../../tysila4 -t x86 -e _start -o kernel.o -L /mnt/d/coreclr netstandard2.0/barebones.dll

iso/kernel.bin: kernel.o
	i686-elf-ld -T linker.ld --gc-sections --undefined=gcmalloc --unresolved-symbols=ignore-all -o iso/kernel.bin loader.o kernel.o 

barebones.iso: iso/kernel.bin iso/boot/grub/grub.cfg
	grub-mkrescue -o barebones.iso iso




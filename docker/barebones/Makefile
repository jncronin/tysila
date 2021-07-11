all: barebones.iso

loader.o: loader.asm
	nasm -felf -o loader.o loader.asm

kernel.exe: kernel.cs
	dotnet build
	cp netstandard2.0/barebones.dll kernel.exe

kernel.o: kernel.exe
	dotnet run --project ../../tysila4 -t x86 -e _start -o kernel.o -L /mnt/d/coreclr kernel.exe

iso/kernel.bin: kernel.o
	i686-elf-ld -T linker.ld --gc-sections --undefined=gcmalloc --unresolved-symbols=ignore-all -o iso/kernel.bin loader.o kernel.o 

barebones.iso: iso/kernel.bin iso/boot/grub/grub.cfg
	grub-mkrescue -o barebones.iso iso




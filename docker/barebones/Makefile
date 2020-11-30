all: barebones.iso

loader.o: loader.asm
	nasm -felf -o loader.o loader.asm

kernel.exe: kernel.cs
	mcs /target:exe /out:kernel.exe /unsafe kernel.cs

kernel.o: kernel.exe
	dotnet run --project ../../tysila4 -t x86 -e _start -o kernel.o -L /mnt/d/coreclr kernel.exe

iso/kernel.bin: kernel.o
	i686-elf-ld -T linker.ld --gc-sections --undefined=gcmalloc --unresolved-symbols=ignore-all -o iso/kernel.bin loader.o kernel.o ../libsupcs.x86.o

barebones.iso: iso/kernel.bin iso/boot/grub/grub.cfg
	grub-mkrescue -o barebones.iso iso




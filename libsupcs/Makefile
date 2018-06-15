TYBUILD=../tybuild/bin/Release/tybuild.exe
TYSILA=../tysila2/bin/Release/tysila2.exe
YASM=yasm

AR=x86_64-elf-ar
RANLIB=x86_64-elf-ranlib

ARM_AR=arm-none-eabi-ar
ARM_RANLIB=arm-none-eabi-ranlib

PELIBSUPCS=bin/Release/libsupcs.dll
LIBSUPCSOBJ=libsupcs.obj
LIBSUPCSA=libsupcs.a

ARM_LIBSUPCSOBJ = libsupcs.arm.obj
ARM_LIBSUPCSA = libsupcs.arm.a

X86_64_INVOKE=x86_64_Invoke.o
X86_64_ARITH=x86_64_arith.o
X86_64_CPU=x86_64_cpu.o

X86_64_ASM=$(X86_64_INVOKE) $(X86_64_ARITH) $(X86_64_CPU)
ARM_ASM=

MSCORLIBDLL=../mono/corlib/mscorlib.dll

TYSILAFLAGS += -q -c -g -L../mono/corlib
TYBUILDFLAGS += /p:Configuration=Release /v /unsafe /tools:3_5 /Wc,warn:0

.PHONY: clean

all: $(LIBSUPCSA)

$(TYBUILD):
	cd ../tybuild && make

$(TYSILA2):
	cd ../tysila2 && make

$(MSCORLIBDLL):
	cd ../mono/corlib && make mscorlib.dll

$(PELIBSUPCS): $(TYBUILD)
	$(TYBUILD) $(TYBUILDFLAGS)

$(LIBSUPCSOBJ): $(PELIBSUPCS) $(TYSILA) $(MSCORLIBDLL)
	$(TYSILA) $(TYSILAFLAGS) -o $(LIBSUPCSOBJ) $(PELIBSUPCS)

$(ARM_LIBSUPCSOBJ): $(PELIBSUPCS) $(TYSILA) $(MSCORLIBDLL)
	$(TYSILA) $(TYSILAFLAGS) --arch arm-elf-tysos -o $(ARM_LIBSUPCSOBJ) $(PELIBSUPCS)

$(X86_64_INVOKE): x86_64_Invoke.asm
	$(YASM) -felf64 -o $@ $<

$(X86_64_ARITH): x86_64_arith.asm
	$(YASM) -felf64 -o $@ $<

$(X86_64_CPU): x86_64_cpu.asm
	$(YASM) -felf64 -o $@ $<

$(LIBSUPCSA): $(LIBSUPCSOBJ) $(X86_64_ASM)
	$(AR) -cru $(LIBSUPCSA) $(LIBSUPCSOBJ) $(X86_64_ASM)
	$(RANLIB) $(LIBSUPCSA)

$(ARM_LIBSUPCSA): $(ARM_LIBSUPCSOBJ) $(ARM_ASM)
	$(ARM_AR) -cru $(ARM_LIBSUPCSA) $(ARM_LIBSUPCSOBJ) $(ARM_ASM)
	$(ARM_RANLIB) $(ARM_LIBSUPCSA)

clean:
	rm -rf obj bin $(PELIBSUPCS) $(LIBSUPCSOBJ) $(X86_64_ASM) $(LIBSUPCSA)


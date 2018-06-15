weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr0_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr0_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr2_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr2_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr3_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr3_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr4_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr4_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_4Lidt_Rv_P1Pv:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Ltr_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_9set_Mxcsr_Rv_P1j:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_9get_Mxcsr_Rj_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_6_Cpuid_Rv_P2jPj:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_RBP_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_RSP_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5RdMsr_Ry_P1j:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5WrMsr_Rv_P2jy:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Tsc_Ry_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Sti_Rv_P0:function
weak _ZX16MemoryOperations_19QuickClearAligned16_Rv_P2yy:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Cli_Rv_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_RBP_Rv_P1y:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Int_Rv_P1h:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_10ReadFSData_RPv_P1i:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_10ReadGSData_RPv_P1i:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_11WriteFSData_Rv_P2iPv:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_11WriteGSData_Rv_P2iPv:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_6Invlpg_Rv_P1y:function
weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_4Exit_Rv_P0:function
weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_4Sgdt_Rv_P1Pv:function
weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_13AsmBreakpoint_Rv_P0:function

weak _ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5Break_Rv_P0:function

weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_4Halt_Rv_P0:function

weak _ZW6System4Math_5Round_Rd_P1d:function
weak _ZW6System4Math_5Floor_Rd_P1d:function

weak __memcpy:function
weak __memmove:function
weak __memset:function
weak __memsetw:function
weak __memcmp:function

weak _conv_u8_r8:function

extern __display_halt
extern __undefined_func

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_13AsmBreakpoint_Rv_P0:
	mov	rax, 0
.L0:
	cmp	rax, 1
	jne	.L0
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr3_Ry_P0:
	mov	rax, cr3
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr3_Rv_P1y:
	mov cr3, rdi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr0_Ry_P0:
	mov	rax, cr0
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr0_Rv_P1y:
	mov cr0, rdi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr2_Ry_P0:
	mov	rax, cr2
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr2_Rv_P1y:
	mov cr2, rdi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Cr4_Ry_P0:
	mov	rax, cr4
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_Cr4_Rv_P1y:
	mov cr4, rdi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_4Lidt_Rv_P1Pv:
	lidt [rdi]
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_4Sgdt_Rv_P1Pv:
	sgdt [rdi]
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Ltr_Rv_P1y:
	ltr di
	ret

_ZX16MemoryOperations_19QuickClearAligned16_Rv_P2yy:
	push rbp
	mov rbp, rsp

	mov rcx, rsi

	shr rcx, 4

	sub rsp, 16
	mov rax, 0x0
	mov [rsp], rax
	mov [rsp + 8], rax

	movdqu	xmm0, [rsp]

.doloop:
	movdqa	[rdi], xmm0
	add	rdi, 16
	loop .doloop
	
	leave
	ret
	
__memcpy:
	push rbp
	mov rbp, rsp

	push 0 ; methodinfo

	mov ecx, edx

	rep movsb

	leave
	ret

__memset:
	push rbp
	mov rbp, rsp

	push 0 ; methodinfo

	mov eax, esi
	mov ecx, edx

	rep stosb

	leave
	ret

__memsetw:
	push rbp
	mov rbp, rsp

	push 0 ; methodinfo

	mov eax, esi
	mov ecx, edx

	rep stosw

	leave
	ret

__memcmp:
	push rbp
	mov rbp, rsp

	push 0 ; methodinfo

	mov ecx, edx
	xor eax, eax

.doloop:
	mov byte al, [rdi]
	sub byte al, [rsi]
	jnz .different
	inc rdi
	inc rsi
	loop .doloop

.different	
	leave
	ret

__memmove:
	push rbp
	mov rbp, rsp

	push 0 ; methodinfo

	mov ecx, edx
	
	cmp rdi, rsi
	ja .domove

	rep movsb
	leave
	ret

.domove:
	add rdi, rcx
	add rsi, rcx

	cmp rcx, 0
	jne .doloop

	leave
	ret

.doloop:
	dec rdi
	dec rsi
	
	mov byte al, [rsi]
	mov byte [rdi], al
	loop .doloop

	leave
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_RBP_Ry_P0:
	mov rax, rbp
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_RSP_Ry_P0:
	mov rax, rsp
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_9set_Mxcsr_Rv_P1j:
	sub rsp, 8
	mov dword [rsp], edi
	ldmxcsr [rsp]
	add rsp, 8
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_9get_Mxcsr_Rj_P0:
	sub rsp, 8
	stmxcsr [rsp]
	mov dword eax, [rsp]
	add rsp, 8
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_6_Cpuid_Rv_P2jPj:
	; void _Cpuid(uint req_no, uint *buf)
	push rbx
	
	xor rcx, rcx
	mov dword eax, edi

	cpuid
	
	mov rdi, rsi
	mov dword [rdi], eax
	mov dword [rdi + 4], ebx
	mov dword [rdi + 8], ecx
	mov dword [rdi + 12], edx

	pop rbx
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5RdMsr_Ry_P1j:
	; static ulong RdMsr(uint reg_no)
	mov dword ecx, edi
	rdmsr
	sub rsp, 8
	mov dword [rsp], eax
	mov dword [rsp + 4], edx
	mov qword rax, [rsp]
	add rsp, 8
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5WrMsr_Rv_P2jy:
	; static void WrMsr(uint reg_no, ulong val)
	mov dword ecx, edi
	mov rax, rsi
	mov rdx, rsi
	shr rdx, 32
	wrmsr
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7get_Tsc_Ry_P0:
	rdtsc
	sub rsp, 8
	mov dword [rsp], eax
	mov dword [rsp + 4], edx
	mov qword rax, [rsp]
	add rsp, 8
	ret

_conv_u8_r8:
	push qword [rsp]
	call __undefined_func
	call __display_halt
.haltloop:
	xchg bx, bx
	hlt
	jmp .haltloop

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_4Exit_Rv_P0:
_ZN14libsupcs#2Edll8libsupcs15OtherOperations_4Halt_Rv_P0:
.l0:
	xchg bx, bx
	pause
	hlt
	jmp .l0

_ZW6System4Math_5Round_Rd_P1d:
	; rely on the mxcsr rounding mode being round-to-even (set up in Arch)

	push		rdi
	cvtsd2si	rax, [rsp]
	cvtsi2sd	xmm0, rax
	pop			rdi

	ret

_ZW6System4Math_5Floor_Rd_P1d:
	; set up rounding mode to floor (towards negative infinity)

	sub rsp, 8
	stmxcsr [rsp]
	mov eax, [rsp]
	mov ecx, eax

	; mask out then set rounding bits
	and eax, 0xffff9fff
	or eax, 0x2000

	; load mxcsr with the new value
	mov [rsp], eax
	ldmxcsr [rsp]

	; do the conversion
	push		rdi
	cvtsd2si	rax, [rsp]
	cvtsi2sd	xmm0, rax
	pop			rdi

	; restore mxcsr
	mov [rsp], ecx
	ldmxcsr [rsp]

	add rsp, 8

	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Sti_Rv_P0:
	sti
	ret
	
_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Cli_Rv_P0:
	cli
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_5Break_Rv_P0:
	int3
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_6Invlpg_Rv_P1y:
	mov rax, rdi
	invlpg [rax]

	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_7set_RBP_Rv_P1y:
	mov rbp, rdi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_10ReadFSData_RPv_P1i:
	mov rax, [fs:rdi]
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_10ReadGSData_RPv_P1i:
	mov rax, [gs:rdi]
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_11WriteFSData_Rv_P2iPv:
	mov [fs:rdi], rsi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_11WriteGSData_Rv_P2iPv:
	mov [gs:rdi], rsi
	ret

_ZN14libsupcs#2Edll17libsupcs#2Ex86_643Cpu_3Int_Rv_P1h:
	mov rax, rdi
	cmp rax, 256
	
	jae .invalid
	shl rax, 2
	mov rcx, .int_list
	add rax, rcx
	jmp rax

.invalid:
	ret

; a table containing int 0, ret, nop, int 1, ret, nop, int 2, ...
.int_list:
%assign i 0
%rep 256
	db 0xcd, i, 0xc3, 0x90
%assign i i+1
%endrep
.end_int_list:

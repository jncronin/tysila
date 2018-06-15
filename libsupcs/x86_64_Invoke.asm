global _ZX11TysosMethod_14InternalInvoke_Ru1O_P3u1IiPv

global __x86_64_invoke
extern memcpy

;        static unsafe extern object asm_invoke(IntPtr meth, int p_length,
;            void* parameters, void* plocs);

;	parameters are passed in rdi, rsi, rdx, rcx, r8, r9
;	we preserve rbp, rbx, r12-r15
__x86_64_invoke:
	push rbp
	mov rbp, rsp

	sub rsp, 0x10

	push r12
	push r13
	push r14
	push r15

	; store method address to rax
	mov	rax, rdi

	; parameters to r12, plocs to r13
	mov r12, rdx
	mov r13, rcx

	; p_length to rbp - 8
	mov [rbp - 8], rsi

	; stack space used is in rbp - 16
	mov	qword [rbp - 16], 0

	; loop on rcx through the parameters
	xor r11, r11

	; r14 and r15 contain next INTEGER and SSE id respectively
	xor r14, r14
	xor r15, r15

	jmp .looptest
.startloop:
	; get ploc to r10
	mov dword r10d, [r13 + r11 * 8];

	cmp r10d, 0
	je .integer
	cmp r10d, 1
	je .integer_unbox
	cmp r10d, 2
	je .sse
	cmp r10d, 4
	je .integer32_unbox	
	cmp r10d, 5
	je .integer_unbox_byref
	jmp .memory

.integer
	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]

	cmp r14, 0
	je .integer_use_rdi
	cmp r14, 1
	je .integer_use_rsi
	cmp r14, 2
	je .integer_use_rdx
	cmp r14, 3
	je .integer_use_rcx
	cmp r14, 4
	je .integer_use_r8
	cmp r14, 5
	je .integer_use_r9
	jmp .integer_use_stack

.integer_use_rdi
	mov rdi, r10
	inc r14
	jmp .paramdone
.integer_use_rsi
	mov rsi, r10
	inc r14
	jmp .paramdone
.integer_use_rdx
	mov rdx, r10
	inc r14
	jmp .paramdone
.integer_use_rcx
	mov rcx, r10
	inc r14
	jmp .paramdone
.integer_use_r8
	mov r8, r10
	inc r14
	jmp .paramdone
.integer_use_r9
	mov r9, r10
	inc r14
	jmp .paramdone
.integer_use_stack
	push r10
	add qword [rbp - 16], 8
	jmp .paramdone

.integer_unbox
	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]
	; unbox it (hardcoded to use m_value offset of 16)
	mov r10, [r10 + 16]
	jmp .integer_unbox_main

.integer32_unbox
	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]
	; unbox it (hardcoded to use m_value offset of 16)
	mov dword r10d, [r10 + 16]
	jmp .integer_unbox_main

.integer_unbox_byref
	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]
	; unbox it (hardcoded to use m_value offset of 16)
	lea r10, [r10 + 16]

.integer_unbox_main
	cmp r14, 0
	je .integer_unbox_use_rdi
	cmp r14, 1
	je .integer_unbox_use_rsi
	cmp r14, 2
	je .integer_unbox_use_rdx
	cmp r14, 3
	je .integer_unbox_use_rcx
	cmp r14, 4
	je .integer_unbox_use_r8
	cmp r14, 5
	je .integer_unbox_use_r9
	jmp .integer_unbox_use_stack

.integer_unbox_use_rdi
	mov rdi, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_rsi
	mov rsi, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_rdx
	mov rdx, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_rcx
	mov rcx, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_r8
	mov r8, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_r9
	mov r9, r10
	inc r14
	jmp .paramdone
.integer_unbox_use_stack
	push r10
	add qword [rbp - 16], 8
	jmp .paramdone

.sse
	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]
	; unbox it (hardcoded to use m_value offset of 16)
	mov r10, [r10 + 16]
	
	cmp r14, 0
	je .sse_use_xmm0
	cmp r15, 1
	je .sse_use_xmm1
	cmp r15, 2
	je .sse_use_xmm2
	cmp r15, 3
	je .sse_use_xmm3
	cmp r15, 4
	je .sse_use_xmm4
	cmp r15, 5
	je .sse_use_xmm5
	cmp r15, 6
	je .sse_use_xmm6
	cmp r15, 7
	je .sse_use_xmm7
	jmp .sse_use_stack

.sse_use_xmm0
	movq xmm0, r10
	inc r15
	jmp .paramdone
.sse_use_xmm1
	movq xmm1, r10
	inc r15
	jmp .paramdone
.sse_use_xmm2
	movq xmm2, r10
	inc r15
	jmp .paramdone
.sse_use_xmm3
	movq xmm3, r10
	inc r15
	jmp .paramdone
.sse_use_xmm4
	movq xmm4, r10
	inc r15
	jmp .paramdone
.sse_use_xmm5
	movq xmm5, r10
	inc r15
	jmp .paramdone
.sse_use_xmm6
	movq xmm6, r10
	inc r15
	jmp .paramdone
.sse_use_xmm7
	movq xmm7, r10
	inc r15
	jmp .paramdone
.sse_use_stack
	push r10
	add qword [rbp - 16], 8
	jmp .paramdone

.memory
	; we are going to call memcpy(rdi, rsi, rdx)
	; get size in r10d
	shr r10, 8

	; store stack space for the object
	sub rsp, r10
	add [rbp - 16], r10

	; store registers
	push rdi
	push rsi
	push rdx
	push r11
	push rax

	; get size in rdx
	mov rdx, r10

	; get reference parameter to r10
	mov r10, [r12 + r11 * 8]
	; load address of unboxed value to rsi (source address)
	lea rsi, [r10 + 20]

	; get dest address to rdi
	lea rdi, [rsp]

	call memcpy

	; restore registers
	pop rax
	pop r11
	pop rdx
	pop rsi
	pop rdi

	jmp .paramdone	

.paramdone
	inc r11
.looptest
	cmp r11, [rbp - 8]
	jl .startloop

	; make the call
	call rax

	add rsp, [rbp - 16]

	pop r15
	pop r14
	pop r13
	pop r12

	leave
	ret

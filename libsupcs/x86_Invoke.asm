global _ZX11TysosMethod_14InternalInvoke_Ru1O_P3u1IiPv

global __x86_invoke
extern memcpy

;        static unsafe extern object asm_invoke(IntPtr meth, int p_length,
;            void* parameters, void* plocs);
;	TODO: rewrite for x86

;	parameters are passed in rdi, rsi, rdx, rcx, r8, r9
;	we preserve rbp, rbx, r12-r15
__x86_invoke:
	hlt
	pause
	jmp __x86_invoke


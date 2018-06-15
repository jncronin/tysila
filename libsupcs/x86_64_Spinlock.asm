weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_10Spinunlock_Rv_P1Pv:function
weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_8Spinlock_Rv_P1Pv:function

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_8Spinlock_Rv_P1Pv:
	xor rax, rax
	mov cl, 1

.doloop:
	lock cmpxchg byte [rdi], cl
	pause
	jnz .doloop

	ret

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_10Spinunlock_Rv_P1Pv:
	mov byte [rdi], 0
	ret



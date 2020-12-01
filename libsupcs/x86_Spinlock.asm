weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_10Spinunlock_Rv_P1Pv:function
weak _ZN14libsupcs#2Edll8libsupcs15OtherOperations_8Spinlock_Rv_P1Pv:function

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_8Spinlock_Rv_P1Pv:
	xor eax, eax
	mov cl, 1
	mov edx, [esp+4]

.doloop:
	lock cmpxchg byte [edx], cl
	pause
	jnz .doloop

	ret

_ZN14libsupcs#2Edll8libsupcs15OtherOperations_10Spinunlock_Rv_P1Pv:
	mov edx, [esp+4]
	mov byte [edx], 0
	ret



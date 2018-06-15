global __signbits_r8
global __signbits_r4
global _conv_u4_r8

extern __halt

__signbits_r8: dq 0x8000000000000000, 0x8000000000000000
__signbits_r4: dq 0x8000000080000000, 0x8000000080000000

_conv_u4_r8:
	call __halt


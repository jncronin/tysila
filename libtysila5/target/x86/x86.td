name = "x86";
nspace = "libtysila5.target." + name;
assembler = name + "_Assembler";
assembler_64 = "x86_64_Assembler";

x86_def = target_init("x86", 4, 0);
x86_64_def = target_init("x86_64", 8, 0);
nspace_64 = x86_64_def.nspace;

next_reg_id = 0;
regs = [ Rstack(), Rcontents(),
	Ri32("eip"), Ri32("eax"), Ri32("ebx"), Ri32("ecx"),
	Ri32("edx"), Ri32("edi"), Ri32("esi"), Ri32("esp"),
	Ri32("ebp"), Rf64("st0"),
	Rf64("xmm0"), Rf64("xmm1"), Rf64("xmm2"), Rf64("xmm3"),
	Rf64("xmm4"), Rf64("xmm5"), Rf64("xmm6"), Rf64("xmm7"),
	MultiReg("eaxedx", [ eax, edx ]) ];
x86_def.regs = regs;

x86_64_def.regs = [ 
	Ralias("eip", "rip", 8),
	Ralias("eax", "rax", 8),
	Ralias("ebx", "rbx", 8),
	Ralias("ecx", "rcx", 8),
	Ralias("edx", "rdx", 8),
	Ralias("esi", "rsi", 8),
	Ralias("edi", "rdi", 8),
	Ri64("r8"),
	Ri64("r9"),
	Ri64("r10"),
	Ri64("r11"),
	Ri64("r12"),
	Ri64("r13"),
	Ri64("r14"),
	Ri64("r15"),
	xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7,
	Rf64("xmm8"), Rf64("xmm9"), Rf64("xmm10"), Rf64("xmm11"), 
	Rf64("xmm12"), Rf64("xmm13"), Rf64("xmm14"), Rf64("xmm15"),
	MultiReg("raxrdx", [ rax, rdx ]) ];

// Where can cli types go?
x86_def.ct_map = 
	[
		[ ct_int32, [ esi, edi, ecx, ebx ] ],
		[ ct_int64, [ ] ],
		[ ct_float, [ xmm1, xmm2, xmm3, xmm4, xmm5, xmm6 ] ],
	];
x86_64_def.ct_map = 
	[
		[ ct_int32, [ r8, r9, r10, r11, r12, r13, r14, r15, rsi, rdi, rcx, rbx ] ],
		[ ct_int64, [ r8, r9, r10, r11, r12, r13, r14, r15, rsi, rdi, rcx, rbx ] ],
		[ ct_float, [ xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm8, xmm9, xmm10, xmm11, xmm12, xmm13, xmm14, xmm15 ] ],
	];

x86_instrs = [
	"mov_rm32_r32",
	"mov_r32_rm32",
	"mov_r8_rm8",
	"mov_r16_rm16",
	"mov_rm32_imm32",
	"mov_r32_lab",
	"mov_lab_r32",
	"mov_rm32_lab",
	"mov_r32_rm32sib",
	"mov_r32_rm32disp",
	"mov_r32_rm16disp",
	"mov_r32_rm8disp",
	"mov_r32_rm32sibscaledisp",
	"mov_rm32disp_imm32",
	"mov_rm16disp_imm32",
	"mov_rm8disp_imm32",
	"mov_rm32disp_r32",
	"mov_rm16disp_r16",
	"mov_rm8disp_r8",
	"movzxbd_r32_rm8sibscaledisp",
	"movzxwd_r32_rm16sibscaledisp",
	"movsxbd_r32_rm8sibscaledisp",
	"movsxwd_r32_rm16sibscaledisp",
	"neg_rm32",
	"not_rm32",
	"cmp_rm32_r32",
	"cmp_r32_rm32",
	"cmp_rm32_imm32",
	"cmp_rm32_imm8",
	"cmp_rm8_imm8",
	"lock_cmpxchg_rm8_r8",
	"lock_cmpxchg_rm32_r32",
	"pause",
	"set_rm32",
	"movsxbd", 	"movsxwd", 	"movzxbd", 	"movzxwd",
	"movsxbd_r32_rm8disp",
	"movzxbd_r32_rm8disp",
	"movsxwd_r32_rm16disp",
	"movzxwd_r32_rm16disp",
	"jmp_rel32",
	"jcc_rel32",
	"call_rel32",
	"call_rm32",
	"ret",
	"pop_r32",
	"pop_rm32",
	"push_r32",
	"push_rm32",
	"push_imm32",
	"add_rm32_imm32",
	"add_rm32_imm8",
	"sub_rm32_imm32",
	"sub_rm32_imm8",
	"add_r32_rm32",
	"add_rm32_r32",
	"sub_r32_rm32",
	"sub_rm32_r32",
	"adc_r32_rm32",
	"adc_rm32_r32",
	"sbb_r32_rm32",
	"sbb_rm32_r32",
	"idiv_rm32",
	"imul_r32_rm32_imm32",
	"imul_r32_rm32",
	"lea_r32",
	"xor_r32_rm32",
	"xor_rm32_r32",
	"and_r32_rm32",
	"and_rm32_r32",
	"or_r32_rm32",
	"or_rm32_r32",
	"sar_rm32_imm8",
	"sal_rm32_cl",
	"sar_rm32_cl",
	"shr_rm32_cl",
	"and_rm32_imm8",
	"and_rm32_imm32",
	"xchg_r32_rm32",
	"xchg_rm32_r32",
	"lock_xchg_rm8ptr_r8",
	"lock_xchg_rm32ptr_r32",
	"out_dx_al",
	"out_dx_ax",
	"out_dx_eax",
	"in_al_dx",
	"in_ax_dx",
	"in_eax_dx",
	"iret",
	"int3",
	"pushf", "popf", "cli",
	"nop",

	"movsd_xmm_xmmm64",
	"movsd_xmm_xmmm64disp",
	"movsd_xmmm64_xmm",
	"movsd_xmmm64disp_xmm",
	"movss_xmm_xmmm32",
	"movss_xmmm32_xmm",
	"movss_xmmm32disp_xmm",
	"cvtsd2si_r32_xmmm64",
	"cvtsi2sd_xmm_rm32",
	"cvtsd2ss_xmm_xmmm64",
	"cvtss2sd_xmm_xmmm32",
	"cvtss2sd_xmm_xmmm32disp",
	"addsd_xmm_xmmm64",
	"subsd_xmm_xmmm64",
	"mulsd_xmm_xmmm64",
	"divsd_xmm_xmmm64",
	"comisd_xmm_xmmm64",
	"ucomisd_xmm_xmmm64",
	"cmpsd_xmm_xmmm64_imm8",
	"roundsd_xmm_xmmm64_imm8",
	"xorpd_xmm_xmmm128",

	// pseudo insts
	"enter_cli", "exit_cli",

	// put x86_64 instructions after mov_r64_imm64
	"mov_r64_imm64",
	"mov_rm64_imm32",
	"mov_r64_rm64",
	"mov_rm64_r64",
	"mov_rm64disp_imm32",
	"mov_r64_rm64disp",
	"mov_r64_rm32disp",
	"mov_r64_rm16disp",
	"mov_r64_rm8disp",
	"mov_rm64disp_r64",
	"movzxbq",
	"movsxbq_r64_rm8disp",
	"movzxbq_r64_rm8disp",
	"movsxwq_r64_rm16disp",
	"movzxwq_r64_rm16disp",
	"cmp_rm64_r64",
	"cmp_r64_rm64",
	"cmp_rm64_imm32",
	"cmp_rm64_imm8",
	"movsxdq_r64_rm64",
	"xor_r64_rm64",
	"xor_rm64_r64",
	"and_r64_rm64",
	"and_rm64_r64",
	"or_r64_rm64",
	"or_rm64_r64",
	"neg_rm64",
	"not_rm64",
	"imul_r64_rm64",
	"idiv_rm64",
	"sal_rm64_cl",
	"sar_rm64_cl",
	"shr_rm64_cl",
	"xchg_r64_rm64",
	"lock_xchg_rm64ptr_r64",
	"sub_rm64_imm8",
	"sub_rm64_imm32",
	"add_rm64_imm8",
	"add_rm64_imm32",
	"lea_r64",
	"add_r64_rm64",
	"add_rm64_r64",
	"sub_r64_rm64",
	"sub_rm64_r64",
	"adc_r64_rm64",
	"adc_rm64_r64",
	"sbb_r64_rm64",
	"sbb_rm64_r64",
	"cvtsi2sd_xmm_rm64",
	"cvtsd2si_r64_xmmm64",
	"and_rm64_imm8",
	"movzxbq_r64_rm8sibscaledisp",
	"movzxwq_r64_rm16sibscaledisp",
	"movsxbq_r64_rm8sibscaledisp",
	"movsxwq_r64_rm16sibscaledisp",
	"movsxdq_r64_rm32sibscaledisp",
	"mov_r64_rm64sibscaledisp",
	"iretq",
	"lock_cmpxchg_rm64_r64",
	"popfq"
];

intmap(nspace, assembler, name, "insts", "init_instrs", x86_instrs);

/* Override some built-in instruction handler names */
instr(x86_def, [ oc_shl ], "shift");
instr(x86_def, [ oc_shr ], "shift");
instr(x86_def, [ oc_shr_un ], "shift");
instr(x86_64_def, [ oc_shl ], "shift");
instr(x86_64_def, [ oc_shr ], "shift");
instr(x86_64_def, [ oc_shr_un ], "shift");

/* Optimize complex instruction sequences */

// the following is produced by string.get_Chars
instr(x86_def, [ oc_ldc, oc_mul, oc_conv, oc_add, oc_ldc, oc_add, oc_ldind ], "getCharSeq");
instr(x86_64_def, [ oc_ldc, oc_mul, oc_conv, oc_add, oc_ldc, oc_add, oc_ldind ], "getCharSeq");

// the following is produced by initobj
instr(x86_def, [ oc_ldc, oc_zeromem ], "ldc_zeromem");
instr(x86_64_def, [ oc_ldc, oc_zeromem ], "ldc_zeromem");

// the following by ldfld/stfld
instr(x86_def, [ oc_ldc, oc_add, oc_ldind ], "ldc_add_ldind");
instr(x86_64_def, [ oc_ldc, oc_add, oc_ldind ], "ldc_add_ldind");
instr(x86_def, [ oc_ldc, oc_add, oc_stind ], "ldc_add_stind");
instr(x86_64_def, [ oc_ldc, oc_add, oc_stind ], "ldc_add_stind");
instr(x86_def, [ oc_ldc, oc_ldc, oc_add, oc_stind ], "ldc_ldc_add_stind");
instr(x86_64_def, [ oc_ldc, oc_ldc, oc_add, oc_stind ], "ldc_ldc_add_stind");

// ldelem
instr(x86_def, [ oc_ldc, oc_mul, oc_stackcopy, oc_ldc, oc_add, oc_ldind, oc_add, oc_ldind, oc_stackcopy ], "ldelem");
instr(x86_64_def, [ oc_ldc, oc_mul, oc_stackcopy, oc_ldc, oc_add, oc_ldind, oc_add, oc_ldind, oc_stackcopy ], "ldelem");


instr(x86_def, [ oc_ldc, oc_add ], "ldc_add");
instr(x86_64_def, [ oc_ldc, oc_add ], "ldc_add");


// x86-specific instructions
instr(x86_def, [ oc_x86_portin ], "portin");
instr(x86_def, [ oc_x86_portout ], "portout");
instr(x86_64_def, [ oc_x86_portin ], "portin");
instr(x86_64_def, [ oc_x86_portout ], "portout");

// Calling conventions
// SysV
cc_sysv = cc(nspace, assembler, "sysv",
[
	[ ct_int32, [ stack ] ],
	[ ct_intptr, [ stack ] ],
	[ ct_int64, [ stack ] ],
	[ ct_object, [ stack ] ],
	[ ct_ref, [ stack ] ],
	[ ct_vt, [ stack ] ],
	[ ct_float, [ stack ] ],
],
eaxreg | ecxreg | edxreg,			// caller preserves
ebxreg | esireg | edireg			// callee preserves
);

cc_ret_sysv = cc(nspace, assembler, "ret_sysv",
[
	[ ct_int32, [ eax ] ],
	[ ct_intptr, [ eax ] ],
	[ ct_object, [ eax ] ],
	[ ct_ref, [ eax ] ],

	[ ct_int64, [ eaxedx ] ],

	[ ct_float, [ st0 ] ],
]);

x86_def.cc = cc_map(nspace, assembler, [
	[ "sysv", cc_sysv ]
]);
x86_def.retcc = retcc_map(nspace, assembler, [
	[ "ret_sysv", cc_ret_sysv ]
]);


x64_sysv_int = [ rdi, rsi, rdx, rcx, r8, r9, stack ];
x64_sysv_sse = [ xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7, stack ];

// Here we define some register classes for the calling convention
intlist(nspace_64, assembler_64, "sysvc", [
	"MEMORY",
	"INTEGER",
	"SSE",
	"SSEUP",
	"X87",
	"X87UP",
	"COMPLEX_X87",
]);

// Define which classes particular cli types belong to
//  We ignore ct_vt because we handle this specially in code
// Then, define the actual calling convention in terms of classes and
//  the registers they use
x64_cmap = [
	[ ct_int32, sysvc_INTEGER ],
	[ ct_intptr, sysvc_INTEGER ],
	[ ct_int64, sysvc_INTEGER ],
	[ ct_object, sysvc_INTEGER ],
	[ ct_ref, sysvc_INTEGER ],
	[ ct_float, sysvc_SSE ],
];
cc_sysv_64 = cc(nspace_64, assembler_64, "sysv",
	x64_cmap,
[
	[ sysvc_INTEGER, x64_sysv_int ],
	[ sysvc_SSE, x64_sysv_sse ],
	[ sysvc_MEMORY, [ stack ] ],
],
	// caller preserves
	raxreg | rcxreg | rdxreg | rsireg | rdireg | r8reg | r9reg | r10reg | r11reg |
	xmm0reg | xmm1reg | xmm2reg | xmm3reg | xmm4reg | xmm5reg | xmm6reg | xmm7reg |
	xmm8reg | xmm9reg | xmm10reg | xmm11reg | xmm12reg | xmm13reg | xmm14reg | xmm15reg
	,

	// callee preserves
	rbxreg | r12reg | r13reg | r14reg | r15reg
);

cc_ret_sysv_64 = cc(nspace_64, assembler_64, "ret_sysv",
[
	[ ct_int32, [ rax ] ],
	[ ct_intptr, [ rax ] ],
	[ ct_object, [ rax ] ],
	[ ct_ref, [ rax ] ],

	[ ct_int64, [ rax ] ],

	[ ct_float, [ xmm0 ] ],
]);

// ISR calling convention
cc_isr_64 = cc(nspace_64, assembler_64, "isr",
	x64_cmap,
	[],
	0,
	raxreg | rbxreg | rcxreg | rdxreg | rsireg | rdireg | rbpreg | rspreg |
	r8reg | r9reg | r10reg | r11reg | r12reg | r13reg | r14reg | r15reg |
	xmm0reg | xmm1reg | xmm2reg | xmm3reg | xmm4reg | xmm5reg | xmm6reg | xmm7reg |
	xmm8reg | xmm9reg | xmm10reg | xmm11reg | xmm12reg | xmm13reg | xmm14reg | xmm15reg
);
cc_ret_isr_64 = cc(nspace_64, assembler_64, "ret_isr", []);

cc_isrec_64 = cc(nspace_64, assembler_64, "isrec",
	x64_cmap,
	[],
	0,
	raxreg | rbxreg | rcxreg | rdxreg | rsireg | rdireg | rbpreg | rspreg |
	r8reg | r9reg | r10reg | r11reg | r12reg | r13reg | r14reg | r15reg |
	xmm0reg | xmm1reg | xmm2reg | xmm3reg | xmm4reg | xmm5reg | xmm6reg | xmm7reg |
	xmm8reg | xmm9reg | xmm10reg | xmm11reg | xmm12reg | xmm13reg | xmm14reg | xmm15reg
);
cc_ret_isrec_64 = cc(nspace_64, assembler_64, "ret_isrec", []);



// put all ccs in a map
x86_64_def.cc = cc_map(nspace_64, assembler_64, [
	[ "sysv", cc_sysv ],
	[ "isr", cc_isr_64 ],
	[ "isrec", cc_isrec_64 ]
]);

x86_64_def.retcc = retcc_map(nspace_64, assembler_64, [
	[ "ret_sysv", cc_ret_sysv ],
	[ "ret_isr", cc_ret_isr_64 ],
	[ "ret_isrec", cc_ret_isrec_64 ]
]);

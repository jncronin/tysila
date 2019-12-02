name = "arm";
nspace = "libtysila5.target." + name;
assembler = name + "_Assembler";

arm_def = target_init("arm", 4, 0);

ResetRegId();
regs = [ Rstack(), Rcontents(),
	Ri32("r0"), Ri32("r1"), Ri32("r2"), Ri32("r3"),
	Ri32("r4"), Ri32("r5"), Ri32("r6"), Ri32("r7"),
	Ri32("r8"), Ri32("r9"), Ri32("r10"), Ri32("r11"),
	Ri32("r12"), 
	Ri32("sp"), Ri32("lr"),
	Rf32("s0"), Rf32("s1"), Rf32("s2"), Rf32("s3"),
	Rf32("s4"), Rf32("s5"), Rf32("s6"), Rf32("s7"),
	Rf32("s8"), Rf32("s9"), Rf32("s10"), Rf32("s11"),
	Rf32("s12"), Rf32("s13"), Rf32("s14"), Rf32("s15"),

	MultiReg("r0r1", [ r0, r1 ])
];
arm_def.regs = regs;

// Where can cli types go?
arm_def.ct_map = 
	[
		[ ct_int32, [ r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12 ] ],
		[ ct_int64, [ ] ],
		[ ct_float, [ s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15 ] ],
	];


arm_instrs = [

];

intmap(nspace, assembler, name, "insts", "init_instrs", arm_instrs);

// calling conventions
arm_eabi_int = [ r0, r1, r2, r3, stack ];
arm_eabi_float = [ s0, s1, s2, s3, s4, s5, s6, s7, stack ];

// Here we define some register classes for the calling convention
intlist(nspace, assembler, "eabic", [
	"MEMORY",
	"INTEGER",
	"FLOAT"
]);

arm_cmap = [
	[ ct_int32, eabic_INTEGER ],
	[ ct_intptr, eabic_INTEGER ],
	[ ct_int64, eabic_INTEGER ],
	[ ct_object, eabic_INTEGER ],
	[ ct_ref, eabic_INTEGER ],
	[ ct_float, eabic_FLOAT ],
];

cc_arm_eabi = cc(nspace, assembler, "eabi",
	arm_cmap,
[
	[ eabic_INTEGER, arm_eabi_int ],
	[ eabic_FLOAT, arm_eabi_float ],
	[ eabic_MEMORY, [ stack ] ],
],
	// caller preserves
	r0reg | r1reg | r2reg | r3reg |
	r12reg |
	s0reg | s1reg | s2reg | s3reg |
	s4reg | s5reg | s6reg | s7reg
	,

	// callee preserves
	r4reg | r5reg | r6reg | r7reg |
	r8reg | r9reg | r10reg | r11reg |
	s8reg | s9reg | s10reg | s11reg |
	s12reg | s13reg | s14reg | s15reg
);

cc_ret_arm_eabi = cc(nspace, assembler, "ret_eabi",
[
	[ ct_int32, [ r0 ] ],
	[ ct_intptr, [ r0 ] ],
	[ ct_object, [ r0 ] ],
	[ ct_ref, [ r0 ] ],

	[ ct_int64, [ r0r1 ] ],

	[ ct_float, [ s0 ] ],
]);

// put all ccs in a map
arm_def.cc = cc_map(nspace, assembler, [
	[ "eabi", cc_arm_eabi ]
]);

arm_def.retcc = retcc_map(nspace, assembler, [
	[ "ret_eabi", cc_ret_arm_eabi ]
]);

// This is a table map file

include "Maps.td";

intmap("libtysila5.ir", "Opcode", "vl", "vl_names", "init_vl", [
	"stack32",
	"lv32",
	"arg32",
	"stack64",
	"lv64",
	"arg64",
	"c32",
	"c64",
	"stack",
	"lv",
	"arg",
	"c",
	"call_target",
	"br_target",
	"cc",
	"str",
	"void",
	"mreg",
	"ts_token",
]);

intmap("libtysila5.ir", "Opcode", "oc", "oc_names", "init_oc", [
	"null",
	"store",
	"add",
	"sub",
	"mul",
	"div",
	"and",
	"or",
	"xor",
	"not",
	"neg",
	"call",
	"callvirt",
	"calli",
	"nop",
	"ret",
	"cmp",
	"br",
	"brif",
	"enter",
	"enter_handler",
	"conv",
	"stind",
	"ldind",
	"ldindzb",
	"ldindzw",
	"ldstr",
	"ldlabcontents",
	"ldlabaddr",
	"stlabcontents",
	"ldfp",
	"ldloca",
	"zeromem",
	"swap",
	"pop",
	"phi",
	"castclass",
	"isinst",
	"endfinally",
	"ldc",
	"ldloc",
	"stloc",
	"rem",
	"ldarg",
	"starg",
	"ldarga",
	"stackcopy",
	"localloc",
	"shr", "shl", "shr_un",
	"switch",
	"ldobja",
	"cctor_runonce",
	"break",
	"mclabel",
	"memcpy",
	"memset",
	"syncvalcompareandswap",
	"syncvalswap",
	"syncvalexchangeandadd",
	"spinlockhint",

	"target_specific",

	"x86_portin",
	"x86_portout",
]);

intmap("libtysila5.ir", "Opcode", "cc", "cc_names", "init_cc", [
	"always",
	"never",
	"eq",
	"ne",
	"gt",
	"ge",
	"lt",
	"le",
	"a",
	"ae",
	"b",
	"be"
]);

function cc_invert_map_handler(array cc, string invert_cc)
{
	print("\t\t\tcc_invert_map[cc_" + cc[0] + "] = cc_" + invert_cc + ";\n");
}

multimap("libtysila5.ir", "Opcode", "cc_invert", "init_cc_invert", [
	[ [ "always" ], "never" ],
	[ [ "never" ], "always" ],
	[ [ "eq" ], "ne" ],
	[ [ "ne" ], "eq" ],
	[ [ "gt" ], "le" ],
	[ [ "ge" ], "lt" ],
	[ [ "lt" ], "ge" ],
	[ [ "le" ], "gt" ],
	[ [ "a" ], "be" ],
	[ [ "ae" ], "b" ],
	[ [ "b" ], "ae" ],
	[ [ "be" ], "a" ]
], funcref cc_invert_map_handler(array, string));

intmap("libtysila5.ir", "Opcode", "ct", "ct_names", "init_ct", [
	"unknown",
	"int32",
	"int64",
	"intptr",
	"tls_int32",
	"tls_int64",
	"tls_intptr",
	"float",
	"object",
	"ref",
	"vt",
]);

// map cil comparisons to ir codes
function cc_single_map_handler(array cil, string ir)
{
	print("\t\t\tcc_single_map[cil.Opcode.SingleOpcodes." + cil[0] + "] = " + ir + ";\n");
}

function cc_double_map_handler(array cil, string ir)
{
	print("\t\t\tcc_double_map[cil.Opcode.DoubleOpcodes." + cil[0] + "] = " + ir + ";\n");
}

multimap("libtysila5.ir", "Opcode", "cc_single_map", "init_cc_single_map", [
	[ [ "beq" ], "cc_eq" ],
	[ [ "bge" ], "cc_ge" ],
	[ [ "bge_un" ], "cc_ae" ],
	[ [ "bgt" ], "cc_gt" ],
	[ [ "bgt_un" ], "cc_a" ],
	[ [ "ble" ], "cc_le" ],
	[ [ "ble_un" ], "cc_be" ],
	[ [ "blt" ], "cc_lt" ],
	[ [ "blt_un" ], "cc_b" ],
	[ [ "bne_un" ], "cc_ne" ],
	[ [ "brfalse"], "cc_eq" ],
	[ [ "brtrue" ], "cc_ne" ],
	[ [ "beq_s" ], "cc_eq" ],
	[ [ "bge_s" ], "cc_ge" ],
	[ [ "bge_un_s" ], "cc_ae" ],
	[ [ "bgt_s" ], "cc_gt" ],
	[ [ "bgt_un_s" ], "cc_a" ],
	[ [ "ble_s" ], "cc_le" ],
	[ [ "ble_un_s" ], "cc_be" ],
	[ [ "blt_s" ], "cc_lt" ],
	[ [ "blt_un_s" ], "cc_b" ],
	[ [ "bne_un_s" ], "cc_ne" ],
	[ [ "brfalse_s"], "cc_eq" ],
	[ [ "brtrue_s" ], "cc_ne" ]
], funcref cc_single_map_handler(array, string));

multimap("libtysila5.ir", "Opcode", "cc_double_map", "init_cc_double_map", [
	[ [ "ceq" ], "cc_eq" ],
	[ [ "cgt" ], "cc_gt" ],
	[ [ "cgt_un" ], "cc_a" ],
	[ [ "clt" ], "cc_lt" ],
	[ [ "clt_un" ], "cc_b" ]
], funcref cc_double_map_handler(array, string));


function oc_pushes_map_handler(array oc, string h)
{
	print("\t\t\toc_pushes_map[oc_" + oc[0] + "] = " + h + ";\n");
}

multimap("libtysila5.ir", "Opcode", "oc_pushes_map", "init_oc_pushes_map", [
	[ [ "call", "callvirt" ], "get_call_rettype" ],
	[ [ "store" ], "get_store_pushtype" ],
	[ [ "add" ], "get_binnumop_pushtype" ],
	[ [ "sub" ], "get_binnumop_pushtype" ],
	[ [ "conv" ], "get_conv_pushtype" ],
	[ [ "ldstr" ] , "get_object_pushtype" ],
], funcref oc_pushes_map_handler(array, string));


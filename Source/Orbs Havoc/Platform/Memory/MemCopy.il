.class public auto ansi abstract sealed beforefieldinit OrbsHavoc.Platform.Memory.CopyBlock
	extends System.Object
{
	.method public hidebysig static void Copy(void* destination, void* source, int32 byteCount) cil managed aggressiveinlining
	{
		.custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = (
			01 00 00 00
		)
		.maxstack 3

		ldarg.0
		ldarg.1
		ldarg.2
		cpblk
		ret
	}
} 
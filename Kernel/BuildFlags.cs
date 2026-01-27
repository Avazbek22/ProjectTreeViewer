namespace DevProjex.Kernel;

public static class BuildFlags
{
	// Build-time switch for Store builds where auto-elevation is not allowed.
	public const bool AllowElevation =
#if DEVPROJEX_STORE
		false;
#else
		true;
#endif
}

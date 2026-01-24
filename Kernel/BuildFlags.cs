namespace ProjectTreeViewer.Kernel;

public static class BuildFlags
{
	// Build-time switch for Store builds where auto-elevation is not allowed.
	public const bool AllowElevation =
#if PTW_STORE
		false;
#else
		true;
#endif
}

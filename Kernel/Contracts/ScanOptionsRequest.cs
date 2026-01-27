using DevProjex.Kernel.Models;

namespace DevProjex.Kernel.Contracts;

public sealed record ScanOptionsRequest(
	string RootPath,
	IgnoreRules IgnoreRules);

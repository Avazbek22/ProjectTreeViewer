using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.SmartIgnore;

public sealed class FrontendArtifactsIgnoreRule : ISmartIgnoreRule
{
	private static readonly string[] MarkerFiles =
	{
		"package.json",
		"package-lock.json",
		"pnpm-lock.yaml",
		"yarn.lock"
	};

	private static readonly string[] FolderNames =
	{
		"dist",
		"build",
		".next",
		".nuxt",
		".turbo",
		".svelte-kit"
	};

	public SmartIgnoreResult Evaluate(string rootPath)
	{
		if (!Directory.Exists(rootPath))
			return new SmartIgnoreResult(
				new HashSet<string>(StringComparer.OrdinalIgnoreCase),
				new HashSet<string>(StringComparer.OrdinalIgnoreCase));

		bool hasMarker = MarkerFiles.Any(marker => File.Exists(Path.Combine(rootPath, marker)));
		if (!hasMarker)
			return new SmartIgnoreResult(
				new HashSet<string>(StringComparer.OrdinalIgnoreCase),
				new HashSet<string>(StringComparer.OrdinalIgnoreCase));

		return new SmartIgnoreResult(
			new HashSet<string>(FolderNames, StringComparer.OrdinalIgnoreCase),
			new HashSet<string>(StringComparer.OrdinalIgnoreCase));
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.SmartIgnore;

public sealed class SmartIgnoreAnalyzer : ISmartIgnoreAnalyzer
{
	private readonly Lazy<SmartIgnoreCatalog> _catalog;

	public SmartIgnoreAnalyzer()
	{
		_catalog = new Lazy<SmartIgnoreCatalog>(LoadCatalog);
	}

	public IReadOnlyList<IgnoreOptionDefinition> Analyze(string rootPath, IReadOnlySet<string> allowedRootFolders)
	{
		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
			return Array.Empty<IgnoreOptionDefinition>();

		if (allowedRootFolders.Count == 0)
			return Array.Empty<IgnoreOptionDefinition>();

		var catalog = _catalog.Value;
		var foundFolderCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var foundFileCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var remainingFolders = new HashSet<string>(catalog.FolderCandidates.Keys, StringComparer.OrdinalIgnoreCase);
		var remainingFiles = new HashSet<string>(catalog.FileCandidates.Keys, StringComparer.OrdinalIgnoreCase);

		bool hasHiddenFolder = false;
		bool hasHiddenFile = false;
		bool hasDotFolder = false;
		bool hasDotFile = false;

		var rootInfo = new DirectoryInfo(rootPath);

		try
		{
			foreach (var file in rootInfo.EnumerateFiles())
			{
				if (!hasDotFile && file.Name.StartsWith(".", StringComparison.Ordinal))
					hasDotFile = true;

				if (!hasHiddenFile && HasHiddenAttribute(file))
					hasHiddenFile = true;

				if (remainingFiles.Remove(file.Name))
					foundFileCandidates.Add(file.Name);
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch
		{
		}

		var pending = new Stack<DirectoryInfo>();
		foreach (var name in allowedRootFolders)
		{
			var path = Path.Combine(rootPath, name);
			if (Directory.Exists(path))
				pending.Push(new DirectoryInfo(path));
		}

		while (pending.Count > 0)
		{
			var current = pending.Pop();
			if (!hasDotFolder && current.Name.StartsWith(".", StringComparison.Ordinal))
				hasDotFolder = true;

			if (!hasHiddenFolder && HasHiddenAttribute(current))
				hasHiddenFolder = true;

			if (remainingFolders.Remove(current.Name))
				foundFolderCandidates.Add(current.Name);

			IEnumerable<FileInfo> files;
			try
			{
				files = current.EnumerateFiles();
			}
			catch (UnauthorizedAccessException)
			{
				continue;
			}
			catch
			{
				continue;
			}

			foreach (var file in files)
			{
				if (!hasDotFile && file.Name.StartsWith(".", StringComparison.Ordinal))
					hasDotFile = true;

				if (!hasHiddenFile && HasHiddenAttribute(file))
					hasHiddenFile = true;

				if (remainingFiles.Remove(file.Name))
					foundFileCandidates.Add(file.Name);
			}

			IEnumerable<DirectoryInfo> subDirs;
			try
			{
				subDirs = current.EnumerateDirectories();
			}
			catch (UnauthorizedAccessException)
			{
				continue;
			}
			catch
			{
				continue;
			}

			foreach (var dir in subDirs)
				pending.Push(dir);
		}

		var options = new List<IgnoreOptionDefinition>();
		foreach (var name in foundFolderCandidates.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
		{
			options.Add(new IgnoreOptionDefinition(
				Id: name,
				Kind: IgnoreOptionKind.NamedFolder,
				DefaultChecked: catalog.FolderCandidates[name]));
		}

		foreach (var name in foundFileCandidates.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
		{
			options.Add(new IgnoreOptionDefinition(
				Id: name,
				Kind: IgnoreOptionKind.NamedFile,
				DefaultChecked: catalog.FileCandidates[name]));
		}

		if (hasHiddenFolder)
		{
			options.Add(new IgnoreOptionDefinition(
				Id: "hidden-folders",
				Kind: IgnoreOptionKind.HiddenFolders,
				DefaultChecked: catalog.Defaults.HiddenFolders));
		}

		if (hasHiddenFile)
		{
			options.Add(new IgnoreOptionDefinition(
				Id: "hidden-files",
				Kind: IgnoreOptionKind.HiddenFiles,
				DefaultChecked: catalog.Defaults.HiddenFiles));
		}

		if (hasDotFolder)
		{
			options.Add(new IgnoreOptionDefinition(
				Id: "dot-folders",
				Kind: IgnoreOptionKind.DotFolders,
				DefaultChecked: catalog.Defaults.DotFolders));
		}

		if (hasDotFile)
		{
			options.Add(new IgnoreOptionDefinition(
				Id: "dot-files",
				Kind: IgnoreOptionKind.DotFiles,
				DefaultChecked: catalog.Defaults.DotFiles));
		}

		return options;
	}

	private static bool HasHiddenAttribute(FileSystemInfo info)
	{
		try
		{
			return info.Attributes.HasFlag(FileAttributes.Hidden);
		}
		catch
		{
			return false;
		}
	}

	private static SmartIgnoreCatalog LoadCatalog()
	{
		var assembly = typeof(ProjectTreeViewer.Assets.Marker).Assembly;
		var resourceName = "ProjectTreeViewer.Assets.IgnoreProfiles.smart-ignore.json";
		using var stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"Smart ignore catalog not found: {resourceName}");

		var catalog = JsonSerializer.Deserialize<SmartIgnoreCatalogDefinition>(stream, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? throw new InvalidOperationException("Smart ignore catalog is empty.");

		return new SmartIgnoreCatalog(
			catalog.FolderCandidates
				?.ToDictionary(c => c.Name, c => c.DefaultChecked, StringComparer.OrdinalIgnoreCase)
				?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
			catalog.FileCandidates
				?.ToDictionary(c => c.Name, c => c.DefaultChecked, StringComparer.OrdinalIgnoreCase)
				?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
			catalog.Defaults ?? new SmartIgnoreDefaults(true, false, true, false));
	}

	private sealed record SmartIgnoreCatalog(
		Dictionary<string, bool> FolderCandidates,
		Dictionary<string, bool> FileCandidates,
		SmartIgnoreDefaults Defaults);

	private sealed record SmartIgnoreCatalogDefinition(
		List<SmartIgnoreCandidate>? FolderCandidates,
		List<SmartIgnoreCandidate>? FileCandidates,
		SmartIgnoreDefaults? Defaults);

	private sealed record SmartIgnoreCandidate(string Name, bool DefaultChecked);

	private sealed record SmartIgnoreDefaults(
		bool HiddenFolders,
		bool HiddenFiles,
		bool DotFolders,
		bool DotFiles);
}

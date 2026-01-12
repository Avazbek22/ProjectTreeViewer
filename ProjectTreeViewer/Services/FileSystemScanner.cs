using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectTreeViewer;

public sealed class FileSystemScanner
{
	public ScanResult<HashSet<string>> GetExtensions(string rootPath, bool ignoreBin, bool ignoreObj, bool ignoreDot)
	{
		var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		bool rootAccessDenied = false;
		bool hadAccessDenied = false;

		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
			return new ScanResult<HashSet<string>>(exts, false, false);

		var pending = new Stack<string>();
		pending.Push(rootPath);

		bool isFirst = true;

		while (pending.Count > 0)
		{
			var dir = pending.Pop();

			IEnumerable<string> subDirs;
			try
			{
				subDirs = Directory.EnumerateDirectories(dir);
			}
			catch (UnauthorizedAccessException)
			{
				hadAccessDenied = true;
				if (isFirst) rootAccessDenied = true;
				continue;
			}
			catch
			{
				continue;
			}

			foreach (var subDir in subDirs)
			{
				var name = Path.GetFileName(subDir);
				if (ShouldSkipDirectory(name, ignoreBin, ignoreObj, ignoreDot))
					continue;

				pending.Push(subDir);
			}

			IEnumerable<string> files;
			try
			{
				files = Directory.EnumerateFiles(dir);
			}
			catch (UnauthorizedAccessException)
			{
				hadAccessDenied = true;
				if (isFirst) rootAccessDenied = true;
				continue;
			}
			catch
			{
				continue;
			}

			foreach (var file in files)
			{
				var name = Path.GetFileName(file);

				if (ignoreDot && name.StartsWith(".", StringComparison.Ordinal))
					continue;

				var ext = Path.GetExtension(name);
				if (!string.IsNullOrWhiteSpace(ext))
					exts.Add(ext);
			}

			isFirst = false;
		}

		return new ScanResult<HashSet<string>>(exts, rootAccessDenied, hadAccessDenied);
	}

	public ScanResult<List<string>> GetRootFolderNames(string rootPath, bool ignoreBin, bool ignoreObj, bool ignoreDot)
	{
		var names = new List<string>();

		bool rootAccessDenied = false;
		bool hadAccessDenied = false;

		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
			return new ScanResult<List<string>>(names, false, false);

		string[] dirs;
		try
		{
			dirs = Directory.GetDirectories(rootPath);
		}
		catch (UnauthorizedAccessException)
		{
			return new ScanResult<List<string>>(names, true, true);
		}
		catch
		{
			return new ScanResult<List<string>>(names, false, false);
		}

		foreach (var dir in dirs)
		{
			var name = Path.GetFileName(dir);

			if (ShouldSkipDirectory(name, ignoreBin, ignoreObj, ignoreDot))
				continue;

			names.Add(name);
		}

		names.Sort(StringComparer.OrdinalIgnoreCase);
		return new ScanResult<List<string>>(names, rootAccessDenied, hadAccessDenied);
	}

	private static bool ShouldSkipDirectory(string name, bool ignoreBin, bool ignoreObj, bool ignoreDot)
	{
		if (ignoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase))
			return true;

		if (ignoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase))
			return true;

		if (ignoreDot && name.StartsWith(".", StringComparison.Ordinal))
			return true;

		return false;
	}
}

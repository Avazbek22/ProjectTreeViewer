using System.IO;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.FileSystem;

public sealed class FileSystemScanner : IFileSystemScanner
{
	public bool CanReadRoot(string rootPath)
	{
		try
		{
			_ = Directory.EnumerateFileSystemEntries(rootPath).GetEnumerator().MoveNext();
			return true;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		catch
		{
			return true;
		}
	}

	public ScanResult<HashSet<string>> GetExtensions(string rootPath, IgnoreRules rules)
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

			string[] subDirs;
			try
			{
				subDirs = Directory.GetDirectories(dir);
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

			foreach (var sd in subDirs)
			{
				var dirInfo = new DirectoryInfo(sd);
				if (ShouldSkipDirectory(dirInfo, rules))
					continue;

				pending.Push(sd);
			}

			string[] files;
			try
			{
				files = Directory.GetFiles(dir);
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
				var fileInfo = new FileInfo(file);
				var name = fileInfo.Name;

				if (ShouldSkipFile(fileInfo, rules))
					continue;

				var ext = Path.GetExtension(name);
				if (!string.IsNullOrWhiteSpace(ext))
					exts.Add(ext);
			}

			isFirst = false;
		}

		return new ScanResult<HashSet<string>>(exts, rootAccessDenied, hadAccessDenied);
	}

	public ScanResult<HashSet<string>> GetRootFileExtensions(string rootPath, IgnoreRules rules)
	{
		var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
			return new ScanResult<HashSet<string>>(exts, false, false);

		bool rootAccessDenied = false;
		bool hadAccessDenied = false;

		string[] files;
		try
		{
			files = Directory.GetFiles(rootPath);
		}
		catch (UnauthorizedAccessException)
		{
			return new ScanResult<HashSet<string>>(exts, true, true);
		}
		catch
		{
			return new ScanResult<HashSet<string>>(exts, false, false);
		}

		foreach (var file in files)
		{
			var fileInfo = new FileInfo(file);
			if (ShouldSkipFile(fileInfo, rules))
				continue;

			var ext = Path.GetExtension(fileInfo.Name);
			if (!string.IsNullOrWhiteSpace(ext))
				exts.Add(ext);
		}

		return new ScanResult<HashSet<string>>(exts, rootAccessDenied, hadAccessDenied);
	}

	public ScanResult<List<string>> GetRootFolderNames(string rootPath, IgnoreRules rules)
	{
		var names = new List<string>();

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
			var dirInfo = new DirectoryInfo(dir);
			if (ShouldSkipDirectory(dirInfo, rules))
				continue;

			names.Add(dirInfo.Name);
		}

		names.Sort(StringComparer.OrdinalIgnoreCase);
		return new ScanResult<List<string>>(names, false, false);
	}

	private static bool ShouldSkipDirectory(DirectoryInfo dirInfo, IgnoreRules rules)
	{
		var name = dirInfo.Name;

		if (rules.SmartIgnoredFolders.Contains(name))
			return true;

		if (rules.IgnoreBinFolders && name.Equals("bin", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreObjFolders && name.Equals("obj", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreDotFolders && name.StartsWith(".", StringComparison.Ordinal))
			return true;

		if (rules.IgnoreHiddenFolders)
		{
			try
			{
				if (dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
					return true;
			}
			catch (IOException)
			{
				// Reserved Windows device names (nul, con, prn, etc.) throw IOException
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return true;
			}
		}

		return false;
	}

	private static bool ShouldSkipFile(FileInfo fileInfo, IgnoreRules rules)
	{
		var name = fileInfo.Name;

		if (rules.SmartIgnoredFiles.Contains(name))
			return true;

		if (rules.IgnoreDotFiles && name.StartsWith(".", StringComparison.Ordinal))
			return true;

		if (rules.IgnoreHiddenFiles)
		{
			try
			{
				if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
					return true;
			}
			catch (IOException)
			{
				// Reserved Windows device names (nul, con, prn, etc.) throw IOException
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return true;
			}
		}

		return false;
	}
}

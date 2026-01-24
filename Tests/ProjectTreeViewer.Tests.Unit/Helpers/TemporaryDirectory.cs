using System;
using System.IO;

namespace ProjectTreeViewer.Tests.Unit.Helpers;

internal sealed class TemporaryDirectory : IDisposable
{
	public TemporaryDirectory()
	{
		Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ProjectTreeViewerTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(Path);
	}

	public string Path { get; }

	public string CreateFile(string relativePath, string content)
	{
		var fullPath = System.IO.Path.Combine(Path, relativePath);
		var dir = System.IO.Path.GetDirectoryName(fullPath);
		if (!string.IsNullOrWhiteSpace(dir))
			Directory.CreateDirectory(dir);
		File.WriteAllText(fullPath, content);
		return fullPath;
	}

	public string CreateBinaryFile(string relativePath, byte[] data)
	{
		var fullPath = System.IO.Path.Combine(Path, relativePath);
		var dir = System.IO.Path.GetDirectoryName(fullPath);
		if (!string.IsNullOrWhiteSpace(dir))
			Directory.CreateDirectory(dir);
		File.WriteAllBytes(fullPath, data);
		return fullPath;
	}

	public string CreateFolder(string relativePath)
	{
		var fullPath = System.IO.Path.Combine(Path, relativePath);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public void Dispose()
	{
		if (Directory.Exists(Path))
			Directory.Delete(Path, recursive: true);
	}
}

using System;
using System.Collections.Generic;
using DevProjex.Kernel.Abstractions;
using DevProjex.Kernel.Models;

namespace DevProjex.Infrastructure.SmartIgnore;

public sealed class CommonSmartIgnoreRule : ISmartIgnoreRule
{
	private static readonly string[] FolderNames =
	{
		".git", ".svn", ".hg",
		".vs", ".idea", ".vscode",
		"node_modules"
	};

	private static readonly string[] FileNames =
	{
		".ds_store",
		"thumbs.db",
		"desktop.ini"
	};

	public SmartIgnoreResult Evaluate(string rootPath)
	{
		var folders = new HashSet<string>(FolderNames, StringComparer.OrdinalIgnoreCase);
		var files = new HashSet<string>(FileNames, StringComparer.OrdinalIgnoreCase);
		return new SmartIgnoreResult(folders, files);
	}
}

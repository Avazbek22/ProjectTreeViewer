using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ISmartIgnoreRule
{
	SmartIgnoreResult Evaluate(string rootPath);
}

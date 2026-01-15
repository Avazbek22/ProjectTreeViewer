namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IIconStore
{
	IReadOnlyCollection<string> Keys { get; }
	byte[] GetIconBytes(string key);
}

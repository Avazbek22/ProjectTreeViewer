using System;
using System.Collections.Generic;
using System.Text.Json;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class EmbeddedDefaultExtensionCatalog : IDefaultExtensionCatalog
{
	private readonly Lazy<IReadOnlyCollection<string>> _extensions;

	public EmbeddedDefaultExtensionCatalog()
	{
		_extensions = new Lazy<IReadOnlyCollection<string>>(LoadExtensions);
	}

	public IReadOnlyCollection<string> GetDefaultExtensions() => _extensions.Value;

	private static IReadOnlyCollection<string> LoadExtensions()
	{
		var assembly = typeof(ProjectTreeViewer.Assets.Marker).Assembly;
		var resourceName = "ProjectTreeViewer.Assets.Defaults.defaultExtensions.json";
		using var stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"Default extensions not found: {resourceName}");

		var payload = JsonSerializer.Deserialize<DefaultExtensionsPayload>(stream, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? throw new InvalidOperationException("Default extensions payload is empty.");

		return payload.Extensions ?? Array.Empty<string>();
	}

	private sealed record DefaultExtensionsPayload(IReadOnlyCollection<string>? Extensions);
}

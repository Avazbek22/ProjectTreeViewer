using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.Application.UseCases;

public sealed class BuildTreeUseCase
{
	private readonly ITreeBuilder _treeBuilder;
	private readonly TreeNodePresentationService _presenter;

	public BuildTreeUseCase(ITreeBuilder treeBuilder, TreeNodePresentationService presenter)
	{
		_treeBuilder = treeBuilder;
		_presenter = presenter;
	}

	public BuildTreeResult Execute(BuildTreeRequest request)
	{
		var result = _treeBuilder.Build(request.RootPath, request.Filter);
		var root = _presenter.Build(result.Root);

		return new BuildTreeResult(root, result.RootAccessDenied, result.HadAccessDenied);
	}
}

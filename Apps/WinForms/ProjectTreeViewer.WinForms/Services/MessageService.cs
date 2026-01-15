using System.Windows.Forms;
using ProjectTreeViewer.Application.Services;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class MessageService
{
	private readonly LocalizationService _localization;

	public MessageService(LocalizationService localization) => _localization = localization;

	public void ShowError(string message)
	{
		MessageBox.Show(message, _localization["Msg.ErrorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowInfo(string message)
	{
		MessageBox.Show(message, _localization["Msg.InfoTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	public void ShowErrorFormat(string key, params object[] args) => ShowError(_localization.Format(key, args));
	public void ShowInfoFormat(string key, params object[] args) => ShowInfo(_localization.Format(key, args));

	public void ShowException(Exception ex)
	{
		ShowError(ex.Message);
	}
}

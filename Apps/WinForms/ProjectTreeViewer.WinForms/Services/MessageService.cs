using System.Windows.Forms;
using ProjectTreeViewer.Application.Services;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class MessageService
{
	// Wraps MessageBox usage so UI text is consistently localized.
	private readonly LocalizationService _localization;

	public MessageService(LocalizationService localization) => _localization = localization;

	public void ShowError(string message)
	{
		// Error dialogs are used for unrecoverable UI problems (bad paths, access denied, etc.).
		MessageBox.Show(message, _localization["Msg.ErrorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowInfo(string message)
	{
		// Info dialogs are used for user-facing warnings (no items selected, empty content, etc.).
		MessageBox.Show(message, _localization["Msg.InfoTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	// Convenience helpers for localized messages with parameters.
	public void ShowErrorFormat(string key, params object[] args) => ShowError(_localization.Format(key, args));
	public void ShowInfoFormat(string key, params object[] args) => ShowInfo(_localization.Format(key, args));

	public void ShowException(Exception ex)
	{
		// For now we surface only the exception message to keep dialogs short.
		ShowError(ex.Message);
	}
}

using System;
using System.Windows.Forms;

namespace ProjectTreeViewer;

public sealed class MessageService
{
	private readonly LocalizationService _l;

	public MessageService(LocalizationService localization) => _l = localization;

	public void ShowError(string message)
	{
		MessageBox.Show(message, _l["Msg.ErrorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowInfo(string message)
	{
		MessageBox.Show(message, _l["Msg.InfoTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	public void ShowErrorFormat(string key, params object[] args) => ShowError(_l.Format(key, args));
	public void ShowInfoFormat(string key, params object[] args) => ShowInfo(_l.Format(key, args));

	public void ShowException(Exception ex)
	{
		ShowError(ex.Message);
	}
}

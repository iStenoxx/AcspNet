﻿namespace AcspNet.Modules.Html
{
	/// <summary>
	/// The HTML message box
	/// Usable template files:
	/// "AcspNet/MessageBox/InfoMessageBox.tpl"
	/// "AcspNet/MessageBox/ErrorMessageBox.tpl"
	/// "AcspNet/MessageBox/OkMessageBox.tpl"
	/// "AcspNet/MessageBox/InlineInfoMessageBox.tpl"
	/// "AcspNet/MessageBox/InlineErrorMessageBox.tpl"
	/// "AcspNet/MessageBox/InlineOkMessageBox.tpl"
	/// Usable <see cref="StringTable"/> items:
	/// "FormTitleMessageBox"
	/// Template variables:
	/// "Message"
	/// "Title"
	/// </summary>
	public interface IMessageBox : IHideObjectMembers
	{
		/// <summary>
		/// Generate message box HTML and set to data collector
		/// </summary>
		/// <param name="text">Text of a message box</param>
		/// <param name="status">Status of a message box</param>
		/// <param name="title">Title of a message box</param>
		void Show(string text, MessageBoxStatus status = MessageBoxStatus.Error, string title = null);

		/// <summary>
		///Generate message box HTML and set to data collector
		/// </summary>
		/// <param name="stringTableItemName">Show message from string table item</param>
		/// <param name="status">Status of a message box</param>
		/// <param name="title">Title of a message box</param>
		void ShowSt(string stringTableItemName, MessageBoxStatus status = MessageBoxStatus.Error, string title = null);

		/// <summary>
		/// Get inline message box HTML
		/// </summary>
		/// <param name="text">Text of a message box</param>
		/// <param name="status">Status of a message box</param>
		/// <returns>Message box html</returns>
		string GetInline(string text, MessageBoxStatus status = MessageBoxStatus.Error);

		/// <summary>
		/// Get inline message box HTML
		/// </summary>
		/// <param name="stringTableItemName">Show message from string table item</param>
		/// <param name="status">Status of a message box</param>
		/// <returns>Message box html</returns>
		string GetInlineSt(string stringTableItemName, MessageBoxStatus status = MessageBoxStatus.Error);
	}
}
﻿namespace AcspNet.Html
{
	public sealed class HtmlContainer : IHtml
	{
		internal IListsGenerator ListsGeneratorInstance;
		internal IMessageBox MessageBoxInstance;

		public IListsGenerator ListsGenerator
		{
			get { return ListsGeneratorInstance; }
		}
		public IMessageBox MessageBox
		{
			get { return MessageBoxInstance; }
		}
	}
}
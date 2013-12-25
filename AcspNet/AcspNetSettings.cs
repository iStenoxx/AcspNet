﻿using System.Collections.Specialized;
using System.Configuration;

namespace AcspNet
{
	/// <summary>
	/// ACSP.NET settings
	/// </summary>
	public sealed class AcspNetSettings
	{
		private readonly string _defaultTemplatesDirInstance = "Templates";
		private readonly string _defaultLanguageInstance = "en";
		private readonly string _extensionsDataDirInstance = "ExtensionsData";
		private readonly string _indexPageInstance = "Index.aspx";
		private readonly bool _templatesMemoryCache;

		/// <summary>
		/// Default templates directory, for example: Templates, default value is "Templates"
		/// </summary>
		public string DefaultTemplatesDir
		{
			get
			{
				return _defaultTemplatesDirInstance;
			}
		}

		/// <summary>
		/// Default site style
		/// </summary>
		public string DefaultStyle { get; private set; }
		
		/// <summary>
		/// Default language, for example: "en", "ru", "de" etc., default value is "en"
		/// </summary>
		public string DefaultLanguage 
		{
			get
			{
				return _defaultLanguageInstance;
			}
		}

		/// <summary>
		/// Extension data path, for example: ExtensionsData, default value is "ExtensionsData"
		/// </summary>
		public string ExtensionDataDir
		{
			get
			{
				return _extensionsDataDirInstance;
			}
		}

		/// <summary>
		/// Site default page
		/// </summary>
		/// <value>
		/// Site default page
		/// </value>
		public string IndexPage
		{
			get
			{
				return _indexPageInstance;
			}
		}

		/// <summary>
		/// Gets a value indicating whether templates memory cache enabled or disabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if templates memory cache enabled; otherwise, <c>false</c>.
		/// </value>
		public bool TemplatesMemoryCache
		{
			get
			{
				return _templatesMemoryCache;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AcspNetSettings"/> class.
		/// </summary>
		internal AcspNetSettings()
		{
			var config = ConfigurationManager.GetSection("AcspNetSettings") as NameValueCollection;

			if (config == null) return;

			if (!string.IsNullOrEmpty(config["DefaultTemplatesDir"]))
				_defaultTemplatesDirInstance = config["DefaultTemplatesDir"];

			if (!string.IsNullOrEmpty(config["DefaultStyle"]))
				DefaultStyle = config["DefaultStyle"];

			if (!string.IsNullOrEmpty(config["DefaultLanguage"]))
				_defaultLanguageInstance = config["DefaultLanguage"];

			if (!string.IsNullOrEmpty(config["ExtensionDataDir"]))
				_extensionsDataDirInstance = config["ExtensionDataDir"];

			if (!string.IsNullOrEmpty(config["IndexPage"]))
				_indexPageInstance = config["IndexPage"];

			if (!string.IsNullOrEmpty(config["TemplatesMemoryCache"]))
				_templatesMemoryCache = bool.Parse(config["TemplatesMemoryCache"]);
		
		}
	}
}

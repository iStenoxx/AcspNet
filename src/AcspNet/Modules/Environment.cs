﻿namespace AcspNet.Modules
{
	/// <summary>
	/// Site environment properties, initialized from <see cref="IAcspNetSettings" />
	/// </summary>
	public sealed class Environment : IEnvironment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Environment"/> class.
		/// </summary>
		/// <param name="sitePhysicalPath">The site physical path.</param>
		/// <param name="settings">The settings.</param>
		public Environment(string sitePhysicalPath, IAcspNetSettings settings)
		{
			sitePhysicalPath = sitePhysicalPath.Replace("\\", "/");

			if (!sitePhysicalPath.EndsWith("/"))
				sitePhysicalPath = sitePhysicalPath + "/";

			SitePhysicalPath = sitePhysicalPath;

			TemplatesPath = settings.DefaultTemplatesPath;
			DataPath = settings.DataPath;
			SiteStyle = settings.DefaultStyle;
			MasterTemplateFileName = settings.DefaultMasterTemplateFileName;
		}

		/// <summary>
		/// Gets the site physical path.
		/// </summary>
		/// <value>
		/// The site physical path.
		/// </value>
		public string SitePhysicalPath { get; private set; }

		/// <summary>
		/// Site current templates directory relative path
		/// </summary>
		public string TemplatesPath { get; set; }

		/// <summary>
		/// Site current templates directory physical path
		/// </summary>
		public string TemplatesPhysicalPath
		{
			get
			{
				return SitePhysicalPath + TemplatesPath + "/";
			}
		}

		/// <summary>
		/// Gets the data path.
		/// </summary>
		/// <value>
		/// The data path.
		/// </value>
		public string DataPath { get; private set; }

		/// <summary>
		/// Gets the data physical path.
		/// </summary>
		/// <value>
		/// The data physical path.
		/// </value>
		public string DataPhysicalPath
		{
			get
			{
				return SitePhysicalPath + DataPath + "/";
			}
		}

		/// <summary>
		/// Site current style
		/// </summary>
		public string SiteStyle { get; set; }

		/// <summary>
		/// Gets or sets the current master page template file name
		/// </summary>
		/// <value>
		/// The name of the master page template file
		/// </value>
		public string MasterTemplateFileName { get; set; }
	}
}
﻿using System.Threading.Tasks;
using Microsoft.Owin;

namespace AcspNet.Core
{
	/// <summary>
	/// Represent static files request handler
	/// </summary>
	public interface IStaticFilesRequestHandler
	{
		/// <summary>
		/// Determines whether current route path is for static file.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		bool IsStaticFileRoutePath(IOwinContext context);

		/// <summary>
		/// Processes the HTTP request for static files.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		Task ProcessRequest(IOwinContext context);
	}
}
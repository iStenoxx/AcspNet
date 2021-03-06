﻿using System;
using AcspNet.Bootstrapper;
using Owin;

namespace AcspNet.Owin
{
	/// <summary>
	/// OWIN IAppBuilder AcspNet extensions
	/// </summary>
	public static class AppBuilderExtensions
	{
		/// <summary>
		/// Adds AcspNet to OWIN pipeline
		/// </summary>
		/// <param name="builder">The OWIN builder.</param>
		/// <returns></returns>
		public static IAppBuilder UseAcspNet(this IAppBuilder builder)
		{
			try
			{
				BootstrapperFactory.CreateBootstrapper().Register();
				return builder.Use<AcspNetOwinMiddleware>();
			}
			catch (Exception e)
			{
				AcspNetOwinMiddleware.ProcessOnException(e);

				throw;
			}
		}
	}
}
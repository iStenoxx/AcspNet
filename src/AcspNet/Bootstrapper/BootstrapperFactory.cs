﻿using System;
using AcspNet.Meta;

namespace AcspNet.Bootstrapper
{
	/// <summary>
	/// AcspNet bootstrapper factory
	/// </summary>
	public static class BootstrapperFactory
	{
		/// <summary>
		/// Creates the bootstrapper.
		/// </summary>
		/// <returns></returns>
		public static BaseAcspNetBootstrapper CreateBootstrapper()
		{
			var userBootstrapperType = AcspTypesFinder.FindTypeDerivedFrom<BaseAcspNetBootstrapper>();
			return userBootstrapperType != null ? (BaseAcspNetBootstrapper)Activator.CreateInstance(userBootstrapperType) : new BaseAcspNetBootstrapper();
		}
	}
}
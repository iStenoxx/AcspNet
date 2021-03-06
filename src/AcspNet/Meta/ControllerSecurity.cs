﻿using System;
using System.Collections.Generic;

namespace AcspNet.Meta
{
	/// <summary>
	/// Provides controller security information
	/// </summary>
	public class ControllerSecurity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ControllerSecurity" /> class.
		/// </summary>
		/// <param name="isAuthorizationRequired">if set to <c>true</c> then indicates whether controller requires user authorization.</param>
		/// <param name="allowedUserRoles">The allowed user roles.</param>
		public ControllerSecurity(bool isAuthorizationRequired = false, string allowedUserRoles = null)
		{
			IsAuthorizationRequired = isAuthorizationRequired;

			if(!string.IsNullOrEmpty(allowedUserRoles))
				AllowedUserRoles = allowedUserRoles.Replace(" ", "").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// Gets a value indicating whether controller requires user authorization.
		/// </summary>
		/// <value>
		/// <c>true</c> if controller requires authorization; otherwise, <c>false</c>.
		/// </value>
		public bool IsAuthorizationRequired { get; private set; }

		/// <summary>
		/// Gets the allowed user roles.
		/// </summary>
		/// <value>
		/// The allowed user roles.
		/// </value>
		public IEnumerable<string> AllowedUserRoles { get; private set; }
	}
}
﻿using System.Collections.Generic;
using AcspNet.ModelBinding.Attributes;

namespace AcspNet.Tests.TestEntities
{
	public class TestModelRequired
	{
		[Required]
		public string Prop1 { get; set; }

		[Required]
		public IList<string> Prop2 { get; set; }
	}
}
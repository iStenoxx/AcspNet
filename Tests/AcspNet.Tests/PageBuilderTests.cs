﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using Simplify.Templates;

namespace AcspNet.Tests
{
	[TestFixture]
	public class PageBuilderTests
	{
		[Test]
		public void Constructor_NullsPassed_ArgumentNullExceptionsThrown()
		{
			Assert.Throws<ArgumentNullException>(() => new PageBuilder(null, null));
			Assert.Throws<ArgumentNullException>(() => new PageBuilder("test", null));
		}

		[Test]
		public void Build_CorrectData_BuildedCorrectly()
		{
			var files = new Dictionary<string, MockFileData>();

			files.Add("Templates/Index.tpl", "<html>{Var1}{Var2}</html>");

			Template.FileSystem = new MockFileSystem(files, "C:/WebSites/FooSite");

			var tf = new TemplateFactory("C:/WebSites/FooSite/Templates", "en", "en");
			var b = new PageBuilder("Index.tpl", tf);

			var data = new Dictionary<string, string>();

			data.Add("Var1", "data1");
			data.Add("Var2", "data2");

			var result = b.Buid(data);

			Assert.AreEqual("<html>data1data2</html>", result);
		}
	}
}

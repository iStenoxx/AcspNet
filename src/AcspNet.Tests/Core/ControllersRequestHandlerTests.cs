﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Claims;
using AcspNet.Core;
using AcspNet.Meta;
using AcspNet.Routing;
using AcspNet.Tests.TestEntities;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using Simplify.DI;

namespace AcspNet.Tests.Core
{
	[TestFixture]
	public class ControllersRequestHandlerTests
	{
		private ControllersRequestHandler _handler;
		private Mock<IControllersAgent> _agent;
		private Mock<IControllersProcessor> _controllersProcessor;

		private readonly IDIContainerProvider _containerProvider = null;
		private Mock<IOwinContext> _context;

		private ControllerMetaData _metaData;
		private readonly IDictionary<string, Object> _routeParameters = new ExpandoObject();

		[SetUp]
		public void Initialize()
		{
			_agent = new Mock<IControllersAgent>();
			_controllersProcessor = new Mock<IControllersProcessor>();
			_handler = new ControllersRequestHandler(_agent.Object, _controllersProcessor.Object);

			_context = new Mock<IOwinContext>();

			_metaData = new ControllerMetaData(typeof(TestController1),
				new ControllerExecParameters(new ControllerRouteInfo("/foo/bar")));

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult(true, _routeParameters));
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData
			});

			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Ok);

			_context.SetupGet(x => x.Request.Path).Returns(new PathString("/foo/bar"));
			_context.SetupGet(x => x.Request.Method).Returns("GET");
			_context.SetupGet(x => x.Authentication).Returns(new Mock<IAuthenticationManager>().Object);
		}

		[Test]
		public void Execute_NoControllersMatchedNo404Controller_404Returned()
		{
			// Assign
			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Http404, result);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
		}

		[Test]
		public void Execute_NoControllersMatchedButHave404Controller_404ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Ok, result);
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void Execute_NoControllersMatchedButHave404ControllerRawResult_404ControllerExecutedRawReturned()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			_controllersProcessor.Setup(
				x =>
					x.Process(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>())).Returns(ControllerResponseResult.RawOutput);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.RawOutput, result);
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void Execute_NoControllersMatchedButHave404ControllerRedirect_404ControllerExecutedRedirectReturned()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			_controllersProcessor.Setup(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>())).Returns(ControllerResponseResult.Redirect);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Redirect, result);
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void Execute_OnlyAnyPageControllerMatchedButHave404Controller_404ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult(true));
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));
			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(true);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Ok, result);
			_agent.Verify(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>()));
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()));

			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void Execute_StandardControllerMatched_Executed()
		{
			// Assign
			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Ok, result);
			_agent.Verify(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>()));
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)));

			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)), Times.Never);

			_controllersProcessor.Verify(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()));
		}

		[Test]
		public void Execute_StandardControllerMatchedReturnsRawData_ReturnedRawDataSubsequentNotExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersProcessor.Setup(
				x =>
					x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters))).Returns(ControllerResponseResult.RawOutput);
			
			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.RawOutput, result);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Once);
		}

		[Test]
		public void Execute_StandardControllerMatchedReturnsRedirect_ReturnedRedirectSubsequentNotExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersProcessor.Setup(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters))).Returns(ControllerResponseResult.Redirect);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Redirect, result);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Once);
		}

		[Test]
		public void Execute_StandardAsyncControllerMatchedReturnsRawData_ReturnedRawDataSubsequentExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersProcessor.Setup(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()))
				.Returns(new List<ControllerResponseResult> {ControllerResponseResult.RawOutput});

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.RawOutput, result);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Exactly(2));
		}

		[Test]
		public void Execute_StandardAsyncControllerMatchedReturnsRedirect_ReturnedRedirectSubsequentExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersProcessor.Setup(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()))
				.Returns(new List<ControllerResponseResult> { ControllerResponseResult.Redirect });

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Redirect, result);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Exactly(2));
		}

		[Test]
		public void Execute_NotAuthenticated_ReturnedHttp401()
		{
			// Assign
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.NotAuthenticated);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Http401, result);
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}

		[Test]
		public void Execute_ForbiddenHave403Controller_403ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Forbidden);
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http403Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Ok, result);
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
			_controllersProcessor.Verify(
				x =>
					x.Process(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}

		[Test]
		public void Execute_ForbiddenNotHave403Controller_Http403Returned()
		{
			// Assign
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Forbidden);

			// Act
			var result = _handler.Execute(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersHandlerResult.Http403, result);
			_controllersProcessor.Verify(x =>
				x.Process(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}
	}
}
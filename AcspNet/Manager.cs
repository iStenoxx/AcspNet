using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.SessionState;

namespace AcspNet
{
	/// <summary>
	///     ACSP.NET main class
	/// </summary>
	public sealed class Manager
	{
		//private const string CookieUserNameFieldName = "AcspUserName";
		//private const string CookieUserPasswordFieldName = "AcspUserPassword";
		//private const string SessionUserAuthenticationStatusFieldName = "AcspAuthenticationStatus";
		//private const string SessionUserIdFieldName = "AcspAunthenticatedUserID";
		private const string IsNewSessionFieldName = "AcspIsNewSession";

		private static List<ExecExtensionMetaContainer> ExecExtensionsMetaContainers = new List<ExecExtensionMetaContainer>();
		private static List<LibExtensionMetaContainer> LibExtensionsMetaContainers = new List<LibExtensionMetaContainer>();

		private static bool IsBatchExtensionsTypesLoaded;
		private static bool IsIndividualExtensionsTypesLoaded;

		private static readonly object Locker = new object();

		private static string SitePhysicalPathContainer = "";

		private static string SiteUrlContainer = "";

		/// <summary>
		///     Gets the connection of  HTTP query string variables
		/// </summary>
		public readonly NameValueCollection QueryString = HttpContext.Current.Request.QueryString;

		/// <summary>
		///     Gets the System.Web.HttpRequest object for the current HTTP request
		/// </summary>
		public readonly HttpRequest Request = HttpContext.Current.Request;

		/// <summary>
		///     Gets the System.Web.HttpResponse object for the current HTTP response
		/// </summary>
		public readonly HttpResponse Response = HttpContext.Current.Response;

		/// <summary>
		///     Gets the System.Web.HttpSessionState object for the current HTTP request
		/// </summary>
		public readonly HttpSessionState Session = HttpContext.Current.Session;

		///// <summary>
		/////     Engine execution end time
		///// </summary>
		//public DateTime EndExecutionTime;

		/// <summary>
		///     Engine execution start time (the time when Manager instance was created)
		/// </summary>
		public DateTime StartExecutionTime;

		////private int _authenticatedUserID = -1;
		private string _currentAction;
		private string _currentMode;

		private IList<IExecExtension> _execExtensionsList;
		//private bool _isExtensionsExecutionStopped;

		private Dictionary<string, bool> _libExtensionsIsInitializedList;
		private IList<ILibExtension> _libExtensionsList;

		/// <summary>
		///     Initialize ACSP .NET engine instance
		/// </summary>
		public Manager()
		{
			if (Request == null)
				throw new AcspNetException("HTTP Request doest not exist.");

			StartExecutionTime = DateTime.Now;

			lock (Locker)
			{
				if (!IsBatchExtensionsTypesLoaded && !IsIndividualExtensionsTypesLoaded)
					CreateMetaContainers();
			}
		}


		/// <summary>
		///     Gets the web-site physical path, for example: C:\inetpub\wwwroot\YourSite
		/// </summary>
		/// <value>
		///     The site physical path.
		/// </value>
		public static string SitePhysicalPath
		{
			get
			{
				if (SitePhysicalPathContainer == "")
				{
					SitePhysicalPathContainer = HttpContext.Current.Request.PhysicalApplicationPath;

					if (SitePhysicalPathContainer != null)
						SitePhysicalPathContainer = SitePhysicalPathContainer.Replace("\\", "/");
				}

				return SitePhysicalPathContainer;
			}
		}

		/// <summary>
		///     Gets the web-site URL, for example: http://yoursite.com/site1/
		/// </summary>
		/// <value>
		///     The site URL.
		/// </value>
		public static string SiteUrl
		{
			get
			{
				if (SiteUrlContainer == "")
				{
					SiteUrlContainer = string.Format("{0}://{1}{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority,
						HttpContext.Current.Request.ApplicationPath);
				}

				return SiteUrlContainer;
			}
		}

		/// <summary>
		///     Indicating whether session was created with the current request
		/// </summary>
		public static bool IsNewSession
		{
			get { return HttpContext.Current.Session[IsNewSessionFieldName] == null; }
		}

		///// <summary>
		/////     Total engine execution time for current request
		///// </summary>
		//public TimeSpan ExecutionTime
		//{
		//	get { return EndExecutionTime.Subtract(StartExecutionTime); }
		//}

		/////// <summary>
		///////     Gets a value indicating whether current web-site client is authenticated as user.
		/////// </summary>
		/////// <value>
		///////     <c>true</c> if current web-site client is authenticated as user; otherwise, <c>false</c>.
		/////// </value>
		////public bool IsAuthenticatedAsUser { get; private set; }

		/////// <summary>
		///////     Gets the authenticated user identifier.
		/////// </summary>
		/////// <value>
		///////     The authenticated user identifier.
		/////// </value>
		////public int AuthenticatedUserID
		////{
		////	get { return _authenticatedUserID; }
		////}

		/////// <summary>
		///////     Gets the name of the authenticated user.
		/////// </summary>
		/////// <value>
		///////     The name of the authenticated user.
		/////// </value>
		////public string AuthenticatedUserName { get; private set; }

		/////// <summary>
		///////     Gets the authenticated user name from cookie.
		/////// </summary>
		/////// <value>
		///////     The authenticated user name from cookie.
		/////// </value>
		////public string UserNameFromCookie
		////{
		////	get
		////	{
		////		HttpCookie cookie = Request.Cookies[CookieUserNameFieldName];

		////		if (cookie != null)
		////			return cookie.Value ?? "";

		////		return null;
		////	}
		////}

		/////// <summary>
		///////     Gets the authenticated user password from cookie.
		/////// </summary>
		/////// <value>
		///////     The authenticated user password from cookie.
		/////// </value>
		////public string UserPasswordFromCookie
		////{
		////	get
		////	{
		////		HttpCookie cookie = Request.Cookies[CookieUserPasswordFieldName];

		////		if (cookie != null)
		////			return cookie.Value ?? "";

		////		return null;
		////	}
		////}

		/// <summary>
		///     Gets the current web-site action (?act=someAction).
		/// </summary>
		/// <value>
		///     The current action (?act=someAction).
		/// </value>
		public string CurrentAction
		{
			get
			{
				if (_currentAction != null) return _currentAction;

				var action = HttpContext.Current.Request.QueryString["act"];

				_currentAction = action ?? "";

				return _currentAction;
			}
		}

		/// <summary>
		///     Gets the current web-site mode (?act=someAction&amp;mode=somMode).
		/// </summary>
		/// <value>
		///     The current mode (?act=someAction&amp;mode=somMode).
		/// </value>
		public string CurrentMode
		{
			get
			{
				if (_currentMode != null) return _currentMode;

				var mode = HttpContext.Current.Request.QueryString["mode"];

				_currentMode = mode ?? "";

				return _currentMode;
			}
		}

		private static void CreateMetaContainers()
		{
			var callingAssembly = Assembly.GetCallingAssembly();
			var assemblyTypes = callingAssembly.GetTypes();

			var containingClass = assemblyTypes.FirstOrDefault(t => t.IsDefined(typeof(LoadExtensionsFromAssemblyOfAttribute), false)) ??
								  assemblyTypes.FirstOrDefault(t => t.IsDefined(typeof(LoadIndividualExtensionsAttribute), false));

			if (containingClass == null)
				throw new AcspNetException("LoadExtensionsFromAssemblyOf attribute not found in your class");

			var batchExtensionsAttributes = containingClass.GetCustomAttributes(typeof(LoadExtensionsFromAssemblyOfAttribute), false);
			var individualExtensionsAttributes = containingClass.GetCustomAttributes(typeof(LoadIndividualExtensionsAttribute), false);

			if (batchExtensionsAttributes.Length <= 1 && individualExtensionsAttributes.Length <= 1)
			{
				if (batchExtensionsAttributes.Length == 1)
				{
					LoadExtensionsFromAssemblyOf(((LoadExtensionsFromAssemblyOfAttribute)batchExtensionsAttributes[0]).Types);
					IsBatchExtensionsTypesLoaded = true;
				}

				if (individualExtensionsAttributes.Length == 1)
				{
					LoadIndividualExtensions(((LoadIndividualExtensionsAttribute)batchExtensionsAttributes[0]).Types);
					IsIndividualExtensionsTypesLoaded = true;
				}

				SortLibraryExtensionsMetaContainers();
				SortExecExtensionsMetaContainers();
			}
			else if (batchExtensionsAttributes.Length > 1)
				throw new Exception("Multiple LoadExtensionsFromAssemblyOf attributes found");
			else if (individualExtensionsAttributes.Length > 1)
				throw new Exception("Multiple LoadIndividualExtensions attributes found");			
		}

		private static void LoadExtensionsFromAssemblyOf(params Type[] types)
		{
			foreach (var assemblyTypes in types.Select(classType => Assembly.GetAssembly(classType).GetTypes()))
			{
				foreach (var t in assemblyTypes.Where(t => t.GetInterface("ILibExtension") != null))
					AddLibExtensionMetaContainer(t);

				foreach (var t in assemblyTypes.Where(t => t.GetInterface("IExtension") != null))
					AddExecExtensionMetaContainer(t);
			}
		}

		private static void LoadIndividualExtensions(params Type[] types)
		{
			foreach (var t in types.Where(t => t.GetInterface("ILibExtension") != null).Where(t => LibExtensionsMetaContainers.All(x => x.ExtensionType != t)))
				AddLibExtensionMetaContainer(t);

			foreach (var t in types.Where(t => t.GetInterface("IExecExtension") != null).Where(t => ExecExtensionsMetaContainers.All(x => x.ExtensionType != t)))
				AddExecExtensionMetaContainer(t);
		}

		private static void AddLibExtensionMetaContainer(Type extensionType)
		{
			LibExtensionsMetaContainers.Add(new LibExtensionMetaContainer(CreateExtensionMetaContainer(extensionType)));
		}

		private static void AddExecExtensionMetaContainer(Type extensionType)
		{
			var action = "";
			var mode = "";
			var runType = ExecExtensionRunType.OnAction;

			var attributes = extensionType.GetCustomAttributes(typeof(ActionAttribute), false);

			if (attributes.Length > 0)
				action = ((ActionAttribute)attributes[0]).Action;

			attributes = extensionType.GetCustomAttributes(typeof(ModeAttribute), false);

			if (attributes.Length > 0)
				mode = ((ModeAttribute)attributes[0]).Mode;

			attributes = extensionType.GetCustomAttributes(typeof(ExecExtensionRunTypeAttribute), false);

			if (attributes.Length > 0)
				runType = ((ExecExtensionRunTypeAttribute)attributes[0]).RunType;

			ExecExtensionsMetaContainers.Add(new ExecExtensionMetaContainer(CreateExtensionMetaContainer(extensionType), action, mode, runType));
		}

		private static ExtensionMetaContainer CreateExtensionMetaContainer(Type extensionType)
		{
			var priority = 0;
			var version = "";

			var attributes = extensionType.GetCustomAttributes(typeof(PriorityAttribute), false);

			if (attributes.Length > 0)
				priority = ((PriorityAttribute)attributes[0]).Priority;

			attributes = extensionType.GetCustomAttributes(typeof(VersionAttribute), false);

			if (attributes.Length > 0)
				version = ((VersionAttribute)attributes[0]).Version;

			return new ExtensionMetaContainer(extensionType, priority, version);
		}

		private static void SortLibraryExtensionsMetaContainers()
		{
			LibExtensionsMetaContainers = LibExtensionsMetaContainers.OrderBy(x => x.Priority).ToList();
		}
		private static void SortExecExtensionsMetaContainers()
		{
			ExecExtensionsMetaContainers = ExecExtensionsMetaContainers.OrderBy(x => x.Priority).ToList();
		}

		/// <summary>
		///     Run ACSP engine
		/// </summary>
		public void Run()
		{
			CreateLibraryExtensionsInstances();
			InitializeLibraryExtensions();

			CreateExecutableExtensionsInstances();
			RunExecutableExtensions();

			Session.Add(IsNewSessionFieldName, "true");
		}
		
		private void CreateLibraryExtensionsInstances()
		{
			_libExtensionsList = new List<ILibExtension>(LibExtensionsMetaContainers.Count);
			_libExtensionsIsInitializedList = new Dictionary<string, bool>(LibExtensionsMetaContainers.Count);

			foreach (var container in LibExtensionsMetaContainers)
			{
				_libExtensionsList.Add((ILibExtension)Activator.CreateInstance(container.ExtensionType));
				_libExtensionsIsInitializedList.Add(container.ExtensionType.Name, false);
			}
		}

		private void InitializeLibraryExtensions()
		{
			foreach (var extension in _libExtensionsList)
			{
				extension.Initialize(this);
				_libExtensionsIsInitializedList[extension.GetType().Name] = true;
			}
		}

		private void CreateExecutableExtensionsInstances()
		{
			_execExtensionsList = new List<IExecExtension>(ExecExtensionsMetaContainers.Count));

			foreach (var container in ExecExtensionsMetaContainers)
			{
				var extension = (IExecExtension) Activator.CreateInstance(container.ExtensionType);

				// Checking execution parameters
				if (container.Action == "")
				{
					if (container.RunType != ExecExtensionRunType.MainPage ||
						container.RunType == ExecExtensionRunType.MainPage && CurrentAction == "")
						_execExtensionsList.Add(extension);
				}
				else
				{
					// todo
					if (container.RunType != ExecExtensionRunType.MainPage &&
						container.Action.ToLower() == CurrentAction.ToLower() &&
						container.Mode.ToLower() == CurrentMode.ToLower())
						_execExtensionsList.Add(extension);
				}
			}
		}

		private void RunExecutableExtensions()
		{
		//	if (_execExtensionsList.Count <= 0) return ManagerResults.ExtensionsExecutionSucceed;

		//	foreach (var extension in _execExtensionsList.Cast<IExtension>().OrderBy(x => x.ExecParams.Priority))
		//	{
		//		if (_isExtensionsExecutionStopped)
		//			return ManagerResults.ExtensionsExecutionStopped;

		//		// Extension deny checking

		//		//if (extension.ExecParams.ProtectionType == ExtensionProtectionTypes.Guest && (IsAuthenticatedAsUser))
		//		//	return ManagerResults.ExtensionDenyErrorGuest;

		//		//if (extension.ExecParams.ProtectionType == ExtensionProtectionTypes.User && !IsAuthenticatedAsUser)
		//		//	return ManagerResults.ExtensionDenyErrorUser;

		//		extension.Invoke(this);
		//	}

			//return ManagerResults.ExtensionsExecutionSucceed;
		}

		///// <summary>
		/////     Stop ACSP subsequent extensions execution
		///// </summary>
		//public void StopExtensionsExecution()
		//{
		//	_isExtensionsExecutionStopped = true;
		//}

		///// <summary>
		/////     Gets library extension instance
		///// </summary>
		///// <typeparam name="T">Library extension instance to get</typeparam>
		///// <returns>Library extension</returns>
		//public T GetLibExtension<T>()
		//	where T : class, ILibExtension
		//{
		//	foreach (var t in _libExtensionsList)
		//	{
		//		var currentType = t.GetType();

		//		if (currentType != typeof (T))
		//			continue;

		//		if (_libExtensionsIsInitializedList[currentType.Name] == false)
		//			throw new AcspNetException("Attempt to call not initialized library extension '" + t.GetType() + "'");

		//		return t as T;
		//	}

		//	return null;
		//}

		///// <summary>
		/////     Redirects a client to a new URL
		///// </summary>
		//public void Redirect(string url)
		//{
		//	StopExtensionsExecution();
		//	Response.Redirect(url, false);
		//}

		/////// <summary>
		/////// Create user authentication cookies (login user via cookies)
		/////// </summary>
		////public void LogInUser(string name, string password, bool autoLogin)
		////{
		////	var cookie = new HttpCookie(CookieUserNameFieldName, name);

		////	if(autoLogin)
		////		cookie.Expires = DateTime.Now.AddDays(256);

		////	Response.Cookies.Add(cookie);

		////	cookie = new HttpCookie(CookieUserPasswordFieldName, password);

		////	if(autoLogin)
		////		cookie.Expires = DateTime.Now.AddDays(256);

		////	Response.Cookies.Add(cookie);
		////}

		/////// <summary>
		/////// Create user authentication variable in user's session (login user via session)
		/////// </summary>
		////public void LogInSessionUser(int userID = -1)
		////{
		////	Session.Add(SessionUserAuthenticationStatusFieldName, "authenticated");
		////	Session.Add(SessionUserIdFieldName, userID);		
		////}

		/////// <summary>
		/////// Checking user cookies authentication data and updating Manager authentication status if success
		/////// </summary>
		////public void AuthenticateUser(int userID, string name, string password)
		////{
		////	var userNameCookie = Request.Cookies[CookieUserNameFieldName];
		////	var userPasswordCookie = Request.Cookies[CookieUserPasswordFieldName];

		////	if(userNameCookie != null &&
		////	   userPasswordCookie != null &&
		////	   userNameCookie.Value == name &&
		////	   userPasswordCookie.Value == password)
		////	{
		////		IsAuthenticatedAsUser = true;
		////		_authenticatedUserID = userID;
		////		AuthenticatedUserName = name;
		////	}
		////	else
		////	{
		////		Request.Cookies.Remove(CookieUserNameFieldName);
		////		Request.Cookies.Remove(CookieUserPasswordFieldName);
		////	}
		////}

		/////// <summary>
		///////Checking user session authentication data and updating Manager authentication status if success
		/////// </summary>
		////public void AuthenticateSessionUser()
		////{
		////	if(Session[SessionUserAuthenticationStatusFieldName] == null || (string)Session[SessionUserAuthenticationStatusFieldName] != "authenticated")
		////		return;

		////	IsAuthenticatedAsUser = true;
		////	_authenticatedUserID = (int)Session[SessionUserIdFieldName];
		////	AuthenticatedUserName = "";
		////}

		/////// <summary>
		/////// Remove user authentication data cookies
		/////// </summary>
		////public void LogOutUser()
		////{
		////	var myCookie = new HttpCookie(CookieUserNameFieldName)
		////					{
		////						Expires = DateTime.Now.AddDays(-1d)
		////					};

		////	Response.Cookies.Add(myCookie);

		////	myCookie = new HttpCookie(CookieUserPasswordFieldName)
		////				{
		////					Expires = DateTime.Now.AddDays(-1d)
		////				};

		////	Response.Cookies.Add(myCookie);

		////	IsAuthenticatedAsUser = false;
		////	_authenticatedUserID = -1;
		////	AuthenticatedUserName = "";
		////}

		/////// <summary>
		/////// Remove user session authentication data
		/////// </summary>
		////public void LogOutSessionUser()
		////{
		////	Session.Remove(SessionUserAuthenticationStatusFieldName);
		////	Session.Remove(SessionUserIdFieldName);
		////}

		///// <summary>
		/////     Gets the library extensions types list.
		///// </summary>
		///// <returns></returns>
		//public static IList<Type> GetLibExtensionsTypesList()
		//{
		//	return LibExtensionsTypes.ToArray();
		//}

		///// <summary>
		/////     Gets the executable extensions types list.
		///// </summary>
		///// <returns></returns>
		//public static IList<Type> GetExecExtensionsTypesList()
		//{
		//	return ExecExtensionsTypes.ToArray();
		//}
	}
}
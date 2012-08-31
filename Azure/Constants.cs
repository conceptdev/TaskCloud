using System;

namespace Azure
{
	public static class Constants
	{
											// i've removed the 'done' filter, because i don't want to hide those tasks for now...
		public static string GetAllUrl = "https://YOUR-SUBDOMAIN.azure-mobile.net/tables/TodoItem"; //?$filter=(complete%20eq%20false)
		public static string AddUrl = "https://YOUR-SUBDOMAIN.azure-mobile.net/tables/TodoItem";
		public static string UpdateUrl = "https://YOUR-SUBDOMAIN.azure-mobile.net/tables/TodoItem/"; // slash on purpose
		public static string MobileServiceAppId = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"; // your application key
	}
}


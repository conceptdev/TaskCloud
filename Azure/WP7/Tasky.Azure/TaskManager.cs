using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MobileServices.Sdk;

namespace TaskyWP7 {
	public static class TaskManager {

		public static void GetTask(int id, TaskDetailsPage page)
		{ 
			MobileServiceTable<TodoItem> todoItemTable = App.MobileServiceClient.GetTable<TodoItem>();

			var query = new MobileServiceQuery()
				.Filter("id eq '"+id+"'")
				.Top(1);

			todoItemTable.Get(query, (res, err) => {
				if (err != null) {
					Console.WriteLine("GET FAILED " + id);
					return; 
				}
				foreach (TodoItem tdi in res) {
					page.Task = tdi; // first one
					return;
				}
			});
		}

		public static void DeleteTask(int taskId)
		{
			MobileServiceTable<TodoItem> todoItemTable = App.MobileServiceClient.GetTable<TodoItem>();

			todoItemTable.Delete(taskId, err => {
				if (err != null) {
					Console.WriteLine("DELETE FAILED");
				}
			});
		}

		public static void SaveTask(TodoItem task)
		{
			MobileServiceTable<TodoItem> todoItemTable = App.MobileServiceClient.GetTable<TodoItem>();

			if (task.Id >= 0) {
				var update = new JObject();
				update["id"] = task.Id;
				update["text"] = task.Text;
				update["complete"] = task.Complete;
				todoItemTable.Update(update, err => {
					if (err != null) {
						Console.WriteLine("UPDATE FAILED");
					}
				});
			} else {
				todoItemTable.Insert(task, (res, err) => {
					if (err != null) {
						Console.WriteLine("INSERT FAILED");
						return;
					}
					Console.WriteLine("SAVED " + res);
				});
			}
		}
	}
}

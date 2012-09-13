using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;


namespace TaskyWP7 {
    public partial class TaskDetailsPage : PhoneApplicationPage {

		TodoItem task;
		public TodoItem Task
		{ get { return task; }
			set
			{
				task = value;

				var vm = new TaskViewModel();
				if (task != null) {
					vm.Update(task);
				}
				DataContext = vm;
			}
		}

        public TaskDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
			DataContext = new TaskViewModel(); // in case we're creating a NEW one

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New) {
              
                if (NavigationContext.QueryString.ContainsKey("id")) {
                    var id = int.Parse(NavigationContext.QueryString["id"]);
					TaskManager.GetTask(id, this);
                }
            }
        }

        private void HandleSave(object sender, EventArgs e)
        {
            var taskvm = (TaskViewModel)DataContext;
            var task = taskvm.GetTask();
            TaskManager.SaveTask(task);
        }

        private void HandleDelete(object sender, EventArgs e)
        {
            var taskvm = (TaskViewModel)DataContext;
			if (taskvm.Id >= 0)
				TaskManager.DeleteTask(taskvm.Id);

            NavigationService.GoBack();
        }
    }
}
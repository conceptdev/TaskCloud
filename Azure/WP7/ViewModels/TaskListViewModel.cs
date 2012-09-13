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
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using MobileServices.Sdk;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace TaskyWP7 {
    public class TaskListViewModel : ViewModelBase {

        public ObservableCollection<TaskViewModel> Items { get; private set; }

        public bool IsUpdating { get; set; }
        public Visibility ListVisibility { get; set; }
        public Visibility NoDataVisibility { get; set; }

        public Visibility UpdatingVisibility
        {
            get
            {
                return (IsUpdating || Items == null || Items.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        Dispatcher dispatcher;

        public void BeginUpdate(Dispatcher dispatcher) {
            this.dispatcher = dispatcher;

            IsUpdating = true;

            ThreadPool.QueueUserWorkItem(delegate {
                List<TodoItem> entries = new List<TodoItem>(); // = TaskManager.GetTasks();
				MobileServiceTable<TodoItem> todoItemTable = App.MobileServiceClient.GetTable<TodoItem>();

				todoItemTable.GetAll((res, err) => {
					if (err != null) {
						//handle it
						return;
					}
					foreach (TodoItem tdi in res) {
						entries.Add(tdi);
					}

					PopulateData(entries);
				});
            });
        }

		void PopulateData(IEnumerable<TodoItem> entries)
        {
            dispatcher.BeginInvoke(delegate {
                //
                // Set all the news items
                //
                Items = new ObservableCollection<TaskViewModel>(
                    from e in entries
                    select new TaskViewModel(e));

                //
                // Update the properties
                //
                OnPropertyChanged("Items");

                ListVisibility = Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                NoDataVisibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                OnPropertyChanged("ListVisibility");
                OnPropertyChanged("NoDataVisibility");
                OnPropertyChanged("IsUpdating");
                OnPropertyChanged("UpdatingVisibility");
            });
        }



    }
}

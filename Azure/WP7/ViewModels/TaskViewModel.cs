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

namespace TaskyWP7 {
    public class TaskViewModel : ViewModelBase {
        public int Id { get; set; }
        public string Text { get; set; }
        //public string Notes { get; set; }
        public bool Complete { get; set; }
    
        public TaskViewModel ()
        {
			Id = -1;
        }
		public TaskViewModel(TodoItem item)
        {
            Update (item);
        }

		public void Update(TodoItem item)
        {
            Id = item.Id;
            Text = item.Text;
            //Notes = item.Notes;
            Complete = item.Complete;
        }

		public TodoItem GetTask()
		{
			return new TodoItem {
                Id = this.Id,
                Text = this.Text,
                //Notes = this.Notes,
                Complete = this.Complete
            };
        }
    }
}

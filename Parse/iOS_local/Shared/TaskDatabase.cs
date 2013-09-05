using System;
using System.Linq;
using System.Collections.Generic;
using SQLite;
using Parse;


// TODO: delete database


namespace Parse
{
	/// <summary>
	/// TaskDatabase builds on SQLite.Net and represents a specific database, in our case, the Task DB.
	/// It contains methods for retrieval and persistance as well as db creation, all based on the 
	/// underlying ORM.
	/// </summary>
	public class TaskDatabase : SQLiteConnection
	{
		static object locker = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="Tasky.DL.TaskDatabase"/> TaskDatabase. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		public TaskDatabase (string path) : base (path)
		{
			// create the tables
			CreateTable<Task> ();
		}
		
		public List<Task> GetItems () 
		{
            lock (locker) {
                return (from i in Table<Task> () select i).ToList ();
            }
		}

		public Task GetItem (int id) 
		{
            lock (locker) {
                return Table<Task>().FirstOrDefault(x => x.Id == id);
            }
		}

		public int SaveItem (Task item)
		{
            lock (locker) {
                if (item.Id > 0) {
                    Update (item);
                    return item.Id;
                } else {
                    return Insert (item);
                }
            }
		}
		
		public int DeleteItem(int id) 
		{
            lock (locker) {
                return Delete<Task> (new Task () { Id = id });
            }
		}
	}
}
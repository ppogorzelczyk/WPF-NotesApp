using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.ViewModel
{
    public class DatabaseHelper
    {
        private static string dbFile = Path.Combine(Environment.CurrentDirectory, "notesDb.db3");

        public static string GetFileLocation()
        {
            return dbFile;
        }

        //public static bool Insert<T>(T item)
        //{
        //    bool result = false;

        //    using (SQLiteConnection conn = new SQLiteConnection(dbFile))
        //    {
        //        conn.CreateTable<T>();
        //        int numberOfRows = conn.Insert(item);
        //        if (numberOfRows > 0)
        //            result = true;
        //    }

        //    return result;
        //}
        public static async Task<T> Insert<T>(T item)
        {
            try
            {
                await App.MobileServiceClient.GetTable<T>().InsertAsync(item);
                return item;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public static bool Update<T>(T item)
        //{
        //    bool result = false;

        //    using (SQLiteConnection conn = new SQLiteConnection(dbFile))
        //    {
        //        conn.CreateTable<T>();
        //        int numberOfRows = conn.Update(item);
        //        if (numberOfRows > 0)
        //            result = true;
        //    }

        //    return result;
        //}
        public static async Task<T> Update<T>(T item)
        {
            try
            {
                await App.MobileServiceClient.GetTable<T>().UpdateAsync(item);
                return item;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool Delete<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(dbFile))
            {
                conn.CreateTable<T>();
                int numberOfRows = conn.Delete(item);
                if (numberOfRows > 0)
                    result = true;
            }

            return result;
        }
    }
}

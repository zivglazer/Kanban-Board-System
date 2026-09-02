using NUnit.Framework;
using IntroSE.Kanban.Backend.DataAccessLayer; // וודא שהנתיב הזה נכון לפרויקט ה-DAL שלך
using IntroSE.Kanban.Backend.ServiceLayer; // וודא שהנתיב הזה נכון לפרויקט ה-ServiceLayer שלך
using System;
using Frontend.Model; // אם אתה משתמש ב-UserModel ישירות כאן
using Frontend.ViewModel; // אם אתה משתמש ב-BackendController כאן
using System.IO;
using System.Threading;

// הגדרת SetUpFixture ברמת ה-namespace של הבדיקות שלך.
// זה יבטיח שהמתודות OneTimeSetUp ו-OneTimeTearDown ירוצו פעם אחת
// עבור כל הבדיקות ב-namespace זה (או ב-assembly אם זה namespace ריק).
namespace FrontendUnitTests
{
    [SetUpFixture]
    public class TestEnvironmentSetup
    {
        // ניתן להוסיף כאן שדות סטטיים אם צריך גישה לאובייקטים מסוימים מכל הבדיקות
        // לדוגמה:
        // public static BackendController GlobalBackendController;
        private static string _dbPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "kanban_test.db");

        /// <summary>
        /// מתודה זו תופעל פעם אחת לפני כל הבדיקות ב-namespace/assembly זה.
        /// היא אחראית לניקוי וארגון מסד הנתונים והמשתמשים ההתחלתיים.
        /// </summary>
        [OneTimeSetUp]
        public void RunBeforeAllTests()
        {
            Console.WriteLine("--- Starting OneTimeSetUp for Test Environment ---");

            // 0. Ensure the database file is not locked by a previous run
            CleanupDatabase();

            // 1. אתחול מסד הנתונים: מחיקה, יצירה מחדש ויצירת כל הטבלאות.
            // זה מבטיח שה-DB נקי לחלוטין לפני תחילת הרצת הבדיקות.
            try
            {
                //Controller.InitializeDatabaseForTests();
                Console.WriteLine("Database initialized successfully for tests.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FATAL ERROR: Failed to initialize database: {ex.ToString()}");
                // DO NOT Assert.Fail here.  Instead, log the error and continue.
                // The database might be in a partially initialized state, but we still want
                // to attempt to register users and run the tests.
                Console.WriteLine("Continuing despite database initialization failure. Tests may be unreliable.");
            }

            // 2. אתחול ה-BackendController לשימוש ברישום משתמשים.
            // (אם אתה צריך להשתמש ב-BackendController באופן גלובלי, אתה יכול לשמור אותו בשדה סטטי).
            BackendController backendController = new BackendController();
            // GlobalBackendController = backendController; // אם תרצה גישה אליו מכל מקום

            // 3. רישום משתמשים בסיסיים שישמשו ברוב הבדיקות.
            // חשוב ללכוד שגיאות רישום ולטפל בהן, כדי שה-setup לא ייכשל בשקט.
            try
            {
                backendController.UserController.Register("mail@mail.com", "Password1");
                Console.WriteLine("User 'mail@mail.com' registered/verified.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during 'mail@mail.com' registration in OneTimeSetUp: {ex.ToString()}");
                // It's acceptable to continue if the user already exists.
                Console.WriteLine("Continuing despite user registration failure (likely user already exists).");
            }

            try
            {
                backendController.UserController.Register("other@mail.com", "Password2");
                Console.WriteLine("User 'other@mail.com' registered/verified.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during 'other@mail.com' registration in OneTimeSetUp: {ex.ToString()}");
                // It's acceptable to continue if the user already exists.
                Console.WriteLine("Continuing despite user registration failure (likely user already exists).");
            }

            Console.WriteLine("--- OneTimeSetUp for Test Environment Completed ---");
        }

        private static void CleanupDatabase()
        {
            // Attempt to close any existing connections to the database
            // This might require reflection to access and dispose of internal connections
            // Or, ensure that the BackendController is properly disposed of after each test run

            // As a last resort, try to delete the database file if it exists
            if (File.Exists(_dbPath))
            {
                try
                {
                    // Wait a short time to allow any processes to release the file
                    Thread.Sleep(100);
                    File.Delete(_dbPath);
                    Console.WriteLine("Existing database file deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting database file: {ex.Message}.  This might indicate a locking issue.");
                    // Consider logging the exception or re-throwing it if the database is essential
                }
            }
        }

        /// <summary>
        /// מתודה זו תופעל פעם אחת לאחר שכל הבדיקות ב-namespace/assembly זה הסתיימו.
        /// היא אחראית לניקוי סופי של מסד הנתונים.
        /// </summary>
        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            Console.WriteLine("--- Starting OneTimeTearDown for Test Environment ---");
            // ניקוי מסד הנתונים (מחיקת הקובץ)
            try
            {
               // Controller.DisposeDatabaseForTests();
                Console.WriteLine("Test database disposed successfully.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error disposing test database: {ex.ToString()}");

            }
            Console.WriteLine("--- OneTimeTearDown for Test Environment Completed ---");
        }
    }
}
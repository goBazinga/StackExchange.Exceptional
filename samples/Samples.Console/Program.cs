﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
#if !COREFX
using StackExchange.Exceptional.Email;
#endif

namespace Samples.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Example of code-only setup, alteratively this can be in the App.config
            // rollupSeconds is 0 so a new file is always generated, for demonstration purposes
            ErrorStore.Setup("Samples.Console", new JSONErrorStore(path: "Errors", rollupSeconds: 0));

            // TODO: Add when System.Net.Mail appears
            //// Example of a code-only email setup, alteratively this can be in the App.config
#if !COREFX
            var emailSettings = new EmailSettings
            {
                FromAddress = "exceptions@site.com",
                FromDisplayName = "Bob the Builder",
                ToAddress = "dont.use@thisadress.com"
            };
            ErrorEmailer.Setup(emailSettings);

            // Optional: for logging all unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += ExceptionalHandler;
#else

#endif

            DisplayExceptionStats();
            PauseForInput();

            try
            {
                throw new Exception("Just a try/catch test");
            }
            catch (Exception ex)
            {
                // logged, but caught so we don't crash
                ErrorStore.Default.Log(ex);
            }

            DisplayExceptionStats();
            PauseForInput();

            System.Console.WriteLine("This next one will crash the program, but will be logged on the way out...");
            PauseForInput();

            // one not explicitly caught, will be logged by ExceptionHandler
            throw new Exception("I am an exception thrown on exit");
        }

#if !COREFX
        // Optional, for logging all unhanled exceptions on the way out
        static void ExceptionalHandler(object sender, UnhandledExceptionEventArgs e)
        {
            // e.ExceptionObject may not be an exception, refer to http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf
            // section 10.5, CLS Rule 40 if you're curious on why this check needs to happen
            var exception = e.ExceptionObject as Exception;

            if (exception != null)
                ErrorStore.Default.Log(exception);
        }
#endif

        static void DisplayExceptionStats()
        {
            System.Console.WriteLine(ErrorStore.Default.Name + " for " + ErrorStore.Default.Name);
            var count = ErrorStore.Default.GetCount();
            System.Console.WriteLine("Exceptions in the log: " + count.ToString());

            var errors = new List<Error>();
            ErrorStore.Default.GetAll(errors);

            if (!errors.Any()) return;

            var last = errors.First();
            System.Console.WriteLine("Latest: " + last.Message + " on " + last.CreationDate.ToString(CultureInfo.CurrentUICulture));
        }

        static void PauseForInput()
        {
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadLine();
        }
    }
}

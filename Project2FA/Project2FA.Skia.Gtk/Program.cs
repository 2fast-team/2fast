﻿using GLib;
using System;
using Uno.UI.Runtime.Skia;

namespace Project2FA.Skia.Gtk
{
    class Program
    {
        static void Main(string[] args)
        {
            ExceptionManager.UnhandledException += delegate (UnhandledExceptionArgs expArgs)
            {
                Console.WriteLine("GLIB UNHANDLED EXCEPTION" + expArgs.ExceptionObject.ToString());
                expArgs.ExitApplication = true;
            };

            var host = new GtkHost(() => new App(), args);

            host.Run();
        }
    }
}

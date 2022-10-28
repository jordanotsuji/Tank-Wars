/// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Windows.Forms;

namespace View
{
    /// <summary>
    /// A class to represent the View of the application.
    /// </summary>
    static class View
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


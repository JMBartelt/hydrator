// // This program sends a windows notification to the user every x minutes.
// // The the user can specify the time interval and the message to be displayed.

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Windows.Forms;
// using System.Threading;

// class Program {

//     static void Main(string[] args) {
//         Console.WriteLine("Enter the time interval in minutes: ");
//         int timeInterval = 30;
//         Console.WriteLine("Enter the message to be displayed: ");
//         string message = "Drink water.";
//         while (true) {
//             Thread.Sleep(timeInterval * 60 * 1000);
//             MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
//         }
//     }
// }

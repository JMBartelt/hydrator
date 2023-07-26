// This program sends a windows notification to the user every x minutes.
// The the user can specify the time interval and the message to be displayed.
class Program {

    static void Main(string[] args) {
        int timeInterval = 30;
        string message = "Drink water.";
        while (true) {
            MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);            
            Thread.Sleep(timeInterval * 60 * 1000);
        }
    }
}

namespace Hydrator;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

public class Program : ApplicationContext
{
    static NotifyIcon notifyIcon = new NotifyIcon();
    static ContextMenuStrip contextMenu;
    static ToolStripMenuItem menuItemSettings;
    static string message = "Drink water.";
    static int timeInterval = 30;

    [STAThread]
    public static void Main()
    {
        Application.Run(new Program());
    }

    public Program()
    {
        notifyIcon.Icon = new Icon("hydrator-icon.ico");

        contextMenu = new ContextMenuStrip();
        menuItemSettings = new ToolStripMenuItem();

        // Context Menu
        contextMenu.Items.AddRange(new ToolStripItem[] { menuItemSettings });

        // Settings Menu
        menuItemSettings.Text = "Settings";
        menuItemSettings.Click += new EventHandler(Settings_Click);

        // Quit
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Quit", null, new EventHandler(delegate
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }));

        // NotifyIcon
        notifyIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
        notifyIcon.ContextMenuStrip = contextMenu;
        notifyIcon.Visible = true;
        
        Thread notificationThread = new Thread(ShowNotification);
        notificationThread.Start();
    }

    private void Settings_Click(object Sender, EventArgs e)
    {
        using (SettingsForm settingsForm = new SettingsForm())
        {
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                message = settingsForm.Message;
                timeInterval = settingsForm.TimeInterval;
            }
        }
    }

    private void ShowNotification()
    {
        while (true)
        {
            Thread.Sleep(timeInterval * 60 * 1000);
            notifyIcon.ContextMenuStrip.Invoke(new MethodInvoker(delegate
            {
                MessageBox.Show(message, "Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }
    }
}

public class SettingsForm : Form
{
    public string Message { get; set; }
    public int TimeInterval { get; set; }

    private TextBox messageBox;
    private NumericUpDown intervalBox;
    private Button okButton;

    public SettingsForm()
    {
        messageBox = new TextBox();
        intervalBox = new NumericUpDown();
        okButton = new Button();

        this.Text = "Settings";

        // Message TextBox
        messageBox.Location = new Point(15, 15);
        messageBox.Width = 250;
        this.Controls.Add(messageBox);

        // Interval NumericUpDown
        intervalBox.Location = new Point(15, 45);
        intervalBox.Width = 50;
        this.Controls.Add(intervalBox);

        // OK Button
        okButton.Location = new Point(190, 75);
        okButton.Text = "OK";
        okButton.Click += new EventHandler(OkButton_Click);
        this.Controls.Add(okButton);
    }

    private void OkButton_Click(object Sender, EventArgs e)
    {
        Message = messageBox.Text;
        TimeInterval = (int)intervalBox.Value;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}

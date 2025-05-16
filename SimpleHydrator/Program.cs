// This program sends a windows notification to the user every x minutes.
// The the user can specify the time interval and the message to be displayed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

class Habit
{
    public string Message { get; set; } = string.Empty;
    public TimeSpan? Interval { get; set; }
    public TimeSpan? TimeOfDay { get; set; }
    public DateTime NextRun { get; set; }
}

class Program
{
    static string HabitsFile = "habits.json";

    static void Main(string[] args)
    {
        var habits = LoadHabits();
        // Update NextRun for all habits on startup
        foreach (var habit in habits)
        {
            if (habit.Interval.HasValue)
                habit.NextRun = DateTime.Now.Add(habit.Interval.Value);
            else if (habit.TimeOfDay.HasValue)
                habit.NextRun = NextTimeTodayOrTomorrow(habit.TimeOfDay.Value);
        }

        var reminderThread = new Thread(() => ReminderLoop(habits)) { IsBackground = true };
        reminderThread.Start();

        while (true)
        {
            Console.WriteLine("\nHabit Tracker Menu:");
            Console.WriteLine("1. View Habits");
            Console.WriteLine("2. Add Habit");
            Console.WriteLine("3. Edit Habit");
            Console.WriteLine("4. Delete Habit");
            Console.WriteLine("5. Exit");
            Console.Write("Select an option: ");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    ViewHabits(habits);
                    break;
                case "2":
                    AddHabit(habits);
                    SaveHabits(habits);
                    break;
                case "3":
                    EditHabit(habits);
                    SaveHabits(habits);
                    break;
                case "4":
                    DeleteHabit(habits);
                    SaveHabits(habits);
                    break;
                case "5":
                    SaveHabits(habits);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    static void ReminderLoop(List<Habit> habits)
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            foreach (var habit in habits)
            {
                if (now >= habit.NextRun)
                {
                    MessageBox.Show(habit.Message, "Habit Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (habit.Interval.HasValue)
                        habit.NextRun = now.Add(habit.Interval.Value);
                    else if (habit.TimeOfDay.HasValue)
                        habit.NextRun = NextTimeTodayOrTomorrow(habit.TimeOfDay.Value);
                }
            }
            Thread.Sleep(1000 * 30); // Check every 30 seconds
        }
    }

    static void ViewHabits(List<Habit> habits)
    {
        if (habits.Count == 0)
        {
            Console.WriteLine("No habits defined.");
            return;
        }
        for (int i = 0; i < habits.Count; i++)
        {
            var h = habits[i];
            string schedule = h.Interval.HasValue
                ? $"Every {h.Interval.Value.TotalMinutes} min"
                : (h.TimeOfDay.HasValue ? $"At {h.TimeOfDay.Value.Hours:D2}:{h.TimeOfDay.Value.Minutes:D2}" : "No schedule");
            Console.WriteLine($"{i + 1}. {h.Message} ({schedule})");
        }
    }

    static void AddHabit(List<Habit> habits)
    {
        Console.Write("Enter habit message: ");
        string msg = Console.ReadLine() ?? string.Empty;
        Console.Write("Interval in minutes (leave blank for specific time): ");
        string intervalStr = Console.ReadLine() ?? string.Empty;
        TimeSpan? interval = null;
        TimeSpan? timeOfDay = null;
        if (!string.IsNullOrWhiteSpace(intervalStr))
        {
            if (double.TryParse(intervalStr, out double mins))
                interval = TimeSpan.FromMinutes(mins);
            else
            {
                Console.WriteLine("Invalid interval.");
                return;
            }
        }
        else
        {
            Console.Write("Time of day (HH:mm): ");
            string timeStr = Console.ReadLine() ?? string.Empty;
            if (TimeSpan.TryParse(timeStr, out TimeSpan tod))
                timeOfDay = tod;
            else
            {
                Console.WriteLine("Invalid time.");
                return;
            }
        }
        var nextRun = interval.HasValue ? DateTime.Now.Add(interval.Value) : (timeOfDay.HasValue ? NextTimeTodayOrTomorrow(timeOfDay.GetValueOrDefault()) : DateTime.Now);
        habits.Add(new Habit { Message = msg, Interval = interval, TimeOfDay = timeOfDay, NextRun = nextRun });
        Console.WriteLine("Habit added.");
    }

    static void EditHabit(List<Habit> habits)
    {
        if (habits.Count == 0)
        {
            Console.WriteLine("No habits to edit.");
            return;
        }
        ViewHabits(habits);
        Console.Write("Enter habit number to edit: ");
        string idxStr = Console.ReadLine() ?? string.Empty;
        if (int.TryParse(idxStr, out int idx) && idx > 0 && idx <= habits.Count)
        {
            var habit = habits[idx - 1];
            Console.Write($"New message (leave blank to keep '{habit.Message}'): ");
            string msg = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(msg))
                habit.Message = msg;
            Console.Write($"New interval in minutes (blank to keep {(habit.Interval.HasValue ? habit.Interval.Value.TotalMinutes.ToString() : "none")}): ");
            string intervalStr = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(intervalStr))
            {
                if (double.TryParse(intervalStr, out double mins))
                {
                    habit.Interval = TimeSpan.FromMinutes(mins);
                    habit.TimeOfDay = null;
                }
                else
                {
                    Console.WriteLine("Invalid interval.");
                    return;
                }
            }
            else if (!habit.Interval.HasValue)
            {
                Console.Write("New time of day (HH:mm): ");
                string timeStr = Console.ReadLine() ?? string.Empty;
                if (TimeSpan.TryParse(timeStr, out TimeSpan tod))
                {
                    habit.TimeOfDay = tod;
                    habit.Interval = null;
                }
                else
                {
                    Console.WriteLine("Invalid time.");
                    return;
                }
            }
            habit.NextRun = habit.Interval.HasValue
                ? DateTime.Now.Add(habit.Interval.Value)
                : (habit.TimeOfDay.HasValue ? NextTimeTodayOrTomorrow(habit.TimeOfDay.GetValueOrDefault()) : DateTime.Now);
            Console.WriteLine("Habit updated.");
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    static void DeleteHabit(List<Habit> habits)
    {
        if (habits.Count == 0)
        {
            Console.WriteLine("No habits to delete.");
            return;
        }
        ViewHabits(habits);
        Console.Write("Enter habit number to delete: ");
        string idxStr = Console.ReadLine() ?? string.Empty;
        if (int.TryParse(idxStr, out int idx) && idx > 0 && idx <= habits.Count)
        {
            habits.RemoveAt(idx - 1);
            Console.WriteLine("Habit deleted.");
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    static List<Habit> LoadHabits()
    {
        if (!File.Exists(HabitsFile))
            return new List<Habit>();
        try
        {
            var json = File.ReadAllText(HabitsFile);
            var habits = JsonSerializer.Deserialize<List<Habit>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return habits ?? new List<Habit>();
        }
        catch
        {
            Console.WriteLine("Failed to load habits. Starting with empty list.");
            return new List<Habit>();
        }
    }

    static void SaveHabits(List<Habit> habits)
    {
        var json = JsonSerializer.Serialize(habits, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(HabitsFile, json);
    }

    static DateTime NextTimeTodayOrTomorrow(TimeSpan timeOfDay)
    {
        DateTime now = DateTime.Now;
        DateTime today = now.Date + timeOfDay;
        if (now < today)
            return today;
        else
            return today.AddDays(1);
    }
}

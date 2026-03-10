using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class TaskItem
{
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string StudentName { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
}

public class InvalidTaskException : Exception
{
    public InvalidTaskException(string message) : base(message) { }
}

class Program
{
    static List<TaskItem> tasks = new List<TaskItem>();
    static Dictionary<int, TaskItem> taskDictionary = new Dictionary<int, TaskItem>();
    static HashSet<string> students = new HashSet<string>();
    static Queue<TaskItem> taskQueue = new Queue<TaskItem>();

    static string filePath = "tasks.json";

    static void Main()
    {
        LoadTasks();

        while (true)
        {
            Console.WriteLine("\n1 Add Task");
            Console.WriteLine("2 Complete Task");
            Console.WriteLine("3 View Tasks");
            Console.WriteLine("4 Overdue Tasks");
            Console.WriteLine("5 Exit");

            Console.Write("Select option: ");
            int choice = int.Parse(Console.ReadLine());

            try
            {
                switch (choice)
                {
                    case 1:
                        AddTask();
                        break;

                    case 2:
                        Console.Write("Enter Task ID: ");
                        int id = int.Parse(Console.ReadLine());
                        Task.Run(() => CompleteTask(id));
                        break;

                    case 3:
                        ViewTasks();
                        break;

                    case 4:
                        ShowOverdueTasks();
                        break;

                    case 5:
                        SaveTasks();
                        return;
                }
            }
            catch (InvalidTaskException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    static void AddTask()
    {
        Console.Write("Task ID: ");
        int id = int.Parse(Console.ReadLine());

        Console.Write("Title: ");
        string title = Console.ReadLine();

        Console.Write("Student Name: ");
        string student = Console.ReadLine();

        Console.Write("Deadline (yyyy-mm-dd): ");
        DateTime deadline = DateTime.Parse(Console.ReadLine());

        if (deadline < DateTime.Now)
            throw new InvalidTaskException("Deadline cannot be in the past");

        TaskItem task = new TaskItem
        {
            TaskId = id,
            Title = title,
            StudentName = student,
            Deadline = deadline,
            IsCompleted = false
        };

        tasks.Add(task);
        taskDictionary[id] = task;
        students.Add(student);
        taskQueue.Enqueue(task);

        Console.WriteLine("Task Added Successfully");
    }

    static void CompleteTask(int id)
    {
        if (!taskDictionary.ContainsKey(id))
            throw new InvalidTaskException("Task not found");

        taskDictionary[id].IsCompleted = true;

        Console.WriteLine($"Task {id} completed by {taskDictionary[id].StudentName}");
    }

    static void ViewTasks()
    {
        foreach (var t in tasks)
        {
            Console.WriteLine($"ID:{t.TaskId} | {t.Title} | {t.StudentName} | Deadline:{t.Deadline} | Completed:{t.IsCompleted}");
        }

        Console.WriteLine("Unique Students: " + students.Count);
    }

    static void ShowOverdueTasks()
    {
        var overdue = tasks.Where(t => t.Deadline < DateTime.Now && !t.IsCompleted);

        foreach (var t in overdue)
        {
            Console.WriteLine($"Overdue Task: {t.Title} for {t.StudentName}");
        }

        int completed = tasks.Count(t => t.IsCompleted);
        Console.WriteLine("Completed Tasks Count: " + completed);
    }

    static void SaveTasks()
    {
        string json = JsonSerializer.Serialize(tasks);
        File.WriteAllText(filePath, json);
    }

    static void LoadTasks()
    {
        if (!File.Exists(filePath))
            return;

        string json = File.ReadAllText(filePath);
        tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);

        foreach (var t in tasks)
        {
            taskDictionary[t.TaskId] = t;
            students.Add(t.StudentName);
            taskQueue.Enqueue(t);
        }
    }
}
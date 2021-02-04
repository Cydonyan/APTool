using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace APTool_v1

    // Ахтунг! Программа не запустится если в папке с ней нет файла trackers.dat либо пустого, либо с некорректным содержанием.
{
    class Tracker
    {
        public string Name { get; set; }
        public double Time { get; set; } // в минутах

        public string Type { get; set; } //Либо program либо site

    }


    class Back
    {
        Tracker[] Trackers;
        int ScanTimer = 60000;
        public Back()
        {

            Trackers = LoadData();
            //Загружаем список отслеживаемых программ
        }
        
        public void Run()
        {
            while (true)
            {
                Scan(Trackers);
                Thread.Sleep(ScanTimer);
            }
            //каждые n секунд смотрим что запущено
        }

        public void Scan(Tracker[] Trackers)
        {
            Tracker[] NewDiscovered = new Tracker[] { };
            foreach (Process process in Process.GetProcesses())
            {
                foreach (Tracker r in Trackers)
                {
                    // проверяем какие из трекеров запущены, если трекер запущен, его временем работы становится время прошедшее с начала запуска процесса.
                    if (r.Name == process.ProcessName && r.Type == "programs")
                    {
                        r.Time = r.Time + 1;
                    }
                    if (process.ProcessName.Contains(r.Name) && r.Type == "sites")
                    {
                        r.Time = r.Time + 1;
                    }

                }
            }
        }
        static Tracker[] LoadData() 
        {
           // загружаем все трекеры в виде json из нашего файла, десериализируем их в объекты класса трекер
            string[] JsonTrackers = File.ReadAllLines("trackers.dat");
            Tracker[] Trackers = new Tracker[JsonTrackers.Length];
            if (JsonTrackers.Length > 0)
            {
                for (int i = 0; i < JsonTrackers.Length; i++)
                {
                    Trackers[i] = JsonSerializer.Deserialize<Tracker>(JsonTrackers[i]);
                }
            }
            return Trackers;
        }

        public void SaveData()
        {
            //сохраняем теркеры
            Tracker[] yeet = LoadData();
            foreach (Tracker a in Trackers)
            {
                // этот на первый взгляд бредовый метод записи времени нужен, потому что для загруженых трекеров время является временем со старта процесса, а не глобальным временем работы за все время.
                foreach (Tracker b in yeet)
                {
                    if ((a.Name == b.Name))
                    {
                        a.Time = a.Time + b.Time;
                    }
                }
            }
            string[] JsonTrackers = new string[Trackers.Length];
            for (int i = 0; i < Trackers.Length; i++)
            {
                JsonTrackers[i] = JsonSerializer.Serialize<Tracker>(Trackers[i]);
            }

            File.WriteAllLines("trackers.dat", JsonTrackers);
            Trackers = LoadData();
            
        }

        public DataTable LoadTable(string type)
        {
            // функция заполнения таблицы содержимым в хорошем порядке.
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Time(Hours)");
            foreach (Tracker t in Trackers)
            {
                if ((t.Type == type) || (type == "all"))
                {
                    DataRow r = dt.NewRow();
                    r["Name"] = t.Name;
                    r["Time(Hours)"] = TimeSpan.FromMinutes(t.Time).ToString(@"hh\:mm");
                    dt.Rows.Add(r);
                }
            }
            return dt;
        }
        public void AddTracker(Tracker r)
        {
            //функция, добавляющая новый трекер к общему списку трекеров.
            Array.Resize<Tracker>(ref Trackers, Trackers.Length + 1);
            Trackers[Trackers.Length - 1] = r;
        }
    }
}

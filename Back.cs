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
                Thread.Sleep(1000);
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
                    if (r.Name == process.ProcessName || r.Name == process.MainWindowTitle.Trim())
                    {
                        r.Time = DateTime.Now.Subtract(process.StartTime).TotalMinutes;

                    }

                }
                //отсюда начинается автоматический отлов сайтов из окна являющегося браузером.
                if (process.MainWindowTitle.ToLower().Contains("firefox")) //у меня Firefox но по идее это схожим образом работает и в остальных браузерах.
                {
                    bool n = true;
                    foreach (Tracker r in Trackers)
                    {
                        //Если сайт уже в списке трекеров то нам больше делать в этом цикле нечего, выходим.
                        if (r.Name == process.MainWindowTitle.Trim())
                        {
                            n = false;
                        }
                    }
                    // Если сайт до этого в списки не попадал, то  создаём для него трекер и кидаем в массив трекеров найденных за это сканирование.
                    if (n)
                    {
                        Array.Resize<Tracker>(ref NewDiscovered, NewDiscovered.Length + 1);
                        NewDiscovered[NewDiscovered.Length - 1] = new Tracker { Name = process.MainWindowTitle.Trim(), Time = 0, Type = "sites" };
                    }
                }
            }
            //добавляем новые трекеры к общему списку трекеров.
            foreach (Tracker t in NewDiscovered)
            {
                AddTracker(t);
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
                    if ((a.Name == b.Name) && (a.Type == "programs"))
                    {
                        a.Time = a.Time + b.Time;
                    }
                }
                //этот костыль отправляет на убой все автоматические отловленные сайты у которых мелоке время, чтобы не замусоривать списки.
                if ((a.Type == "sites") && !(a.Time >= 5))
                {
                    a.Type = "delete";
                }
            }
            // этот блок кода непосредственно убивает отмеченные трекеры и записывает осатльные в файл.
            Trackers = Array.FindAll<Tracker>(Trackers, delegate (Tracker t) { return t.Type != "delete"; });
            string[] JsonTrackers = new string[Trackers.Length];
            for (int i = 0; i < Trackers.Length; i++)
            {
                if (Trackers[i].Type == "delete")
                {
                    JsonTrackers[i] = "";
                }
                else
                {
                    JsonTrackers[i] = JsonSerializer.Serialize<Tracker>(Trackers[i]);
                }
            }
            JsonTrackers = Array.FindAll<string>(JsonTrackers, delegate (string s) { return s != ""; });
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

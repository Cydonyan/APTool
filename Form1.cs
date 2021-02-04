using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace APTool_v1
{
    public partial class Form1 : Form
    {
        Thread Backing;
        Back b;
        DataTable dt;
        string txtResult;
        public Form1()
        {
            InitializeComponent();
            //запуск отслеживателя в отдельном процессе, чтобы он мог действовать независимо от интерфейса, но при этом они могли взаимодействовать.
            b = new Back();
            Backing = new Thread(new ThreadStart(b.Run));
            Backing.Start();
            //Подгружаем таблицу трекеров, по умолчанию отображаем все типы трекеров.
            dt = b.LoadTable("all");
            dataGridView1.DataSource = dt;
            comboBox1.DataSource = new string[] { "programs", "sites" };
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //функция из Скайпа когда закрытие окна сворачивает программу в трей.
            Hide();
            e.Cancel = true;
            notifyIcon1.Visible = true;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            //разворачиваем программу из трея по двойному щелчку
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            // при выходе сохраняем всё и убиваем процесс отcлеживателя
            notifyIcon1.Icon = null;// попытка пофиксить проблему с неисчезающей иконкой в трее
            b.SaveData();
            Backing.Abort();
            System.Environment.Exit(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem)
            {
                case "programs":
                    // диалог добавления нового трекера - программы
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string path = folderBrowserDialog1.SelectedPath;
                        foreach (string f in Directory.EnumerateFiles(path, "*.exe", SearchOption.AllDirectories))
                        {
                            b.AddTracker(new Tracker { Name = Path.GetFileName(f), Time = 0, Type = "programs" });
                        }
                        b.SaveData();
                    }
                    break;
                case "sites":
                    Form2 testDialog = new Form2();
                    if (testDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        this.txtResult = testDialog.textBox1.Text;
                    }
                    else
                    {
                        this.txtResult = "";
                    }
                    testDialog.Dispose();
                    b.AddTracker(new Tracker { Name = txtResult, Time = 0, Type = "sites" });
                    b.SaveData();
                    break;
            }
            UpdateTable();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public void UpdateTable()
        {
            // обновляем таблицу
            dataGridView1.DataSource = b.LoadTable((comboBox1.SelectedItem).ToString()); ;
            dataGridView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TODO удаление трекеров из списка
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //изменение типов отображаемых в таблице трекеров
            dataGridView1.DataSource = b.LoadTable((comboBox1.SelectedItem).ToString());
            dataGridView1.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateTable();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Поиск_файла
{
    /// <summary>
    /// Логика взаимодействия для FileWindow.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        public FileWindow()
        {
            InitializeComponent();
            listBoxFiles.Items.Add("Тут будут файлы для работы из выбранной папки");
            buttonShowFiles.Visibility = Visibility.Hidden;
        }

        // Выбрать каталог
        private void buttonChooseDirectory_Click(object sender, RoutedEventArgs e)
        {
            DialogSelectFolder SelectFolder = new DialogSelectFolder();
            SelectFolder.ShowDialog();
            this.textBlockDefault_Folder.Text = csPathToFolder.PathOfSelectedFolder;
            buttonShowFiles_Click(this, null);
        }

        // Закрыть приложение
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Показать doc файлы
        private void buttonShowFiles_Click(object sender, RoutedEventArgs e)
        {
            // Получить файлы из Выбранного каталога
            string SelectFolder = csPathToFolder.PathOfSelectedFolder;
            if (SelectFolder != null)
            {
                string[] filesdoc = Directory.GetFileSystemEntries(SelectFolder, "*.doc");
                string[] filesdocx = Directory.GetFileSystemEntries(SelectFolder, "*.docx");
                string[] filestxt = Directory.GetFileSystemEntries(SelectFolder, "*.txt");
                string[] filesxls = Directory.GetFileSystemEntries(SelectFolder, "*.xls");
                // Показываем с какими файлами работаем
                listBoxFiles.Items.Clear();
                foreach (string filesFinded in filesdoc)
                {
                    listBoxFiles.Items.Add(filesFinded);
                }
                foreach (string filesFinded in filesdocx)
                {
                    listBoxFiles.Items.Add(filesFinded);
                }
                foreach (string filesFinded in filestxt)
                {
                    listBoxFiles.Items.Add(filesFinded);
                }
                foreach (string filesFinded in filesxls)
                {
                    listBoxFiles.Items.Add(filesFinded);
                }
            }
            else
                MessageBox.Show("Выберите папку из списка!");
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = listBoxFiles.SelectedIndex;
            string file_folder = "";

            file_folder = listBoxFiles.Items[row_index].ToString();
            
            csPathToFolder.PathOfSelectedFolder = file_folder;
            this.Close();
        }

        private void buttonChooseFile_Click(object sender, RoutedEventArgs e)
        {
            int row_index = listBoxFiles.SelectedIndex;
            string file_folder = "";
            try
            {
                file_folder = listBoxFiles.Items[row_index].ToString();
            }
            catch
            {
                MessageBox.Show("Файл не выбран!");
            }
            csPathToFolder.PathOfSelectedFolder = file_folder;
            this.Close();
        }
    }

    // Путь к папке, общий для всех классов
    public static class csPathToFolder
    {
        public static string PathOfSelectedFolder { get; set; }
        public static string SelectedDrive { get; set; }

    }
}

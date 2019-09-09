using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;
using System.Text.RegularExpressions;

namespace Clothing_Industry_WPF.Поиск_файла
{
    /// <summary>
    /// Логика взаимодействия для DialogSelectFolder.xaml
    /// </summary>
    public partial class DialogSelectFolder : Window
    {
        public DialogSelectFolder()
        {
            InitializeComponent();

            // При вызове показывать Заполнить Листбокс, списком локальных дисков
            GetAddDrivesToListBox();
        }



        // Отмена
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Я Выбрал
        private void buttonChoose_Click(object sender, RoutedEventArgs e)
        {
            if (csPathToFolder.PathOfSelectedFolder != null)
            {
                csPathToFolder.PathOfSelectedFolder = textBlockDirectory.Text;
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите каталог", "Выберите каталог", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        // На уровыень выше
        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = csPathToFolder.PathOfSelectedFolder;
            int curPathLength = currentPath.Length;
            // Определяем количество слэшей в пути
            string pattern = "\\";
            int slashCount = new Regex(Regex.Escape(pattern)).Matches(currentPath).Count;
            // MessageBox.Show(amount.ToString()); C:/Work/folder - 2
            if (currentPath == null)
            {
                // GetAddDrivesToListBox(); // Если диск не выбран - показываем диски
            }

            else
            {
                // C:/ Если слэш один и путь состоит из названия диска - Показываем диски
                if ((slashCount <= 1) && (curPathLength <= 3))
                {
                    GetAddDrivesToListBox();
                }
                // C:/Work Если слэш один, но это верхняя папка на диске
                else if ((slashCount <= 1) && (curPathLength > 3))
                {
                    // находим слэш и удаляем знаки до конца строки C:/Work -> C:/
                    int slashZnak = currentPath.IndexOf('\\') + 1;
                    currentPath = currentPath.Remove(slashZnak);
                    csPathToFolder.PathOfSelectedFolder = null; // очищаем значение
                    csPathToFolder.PathOfSelectedFolder = currentPath;
                    textBlockDirectory.Text = csPathToFolder.PathOfSelectedFolder;
                    GetAddItemsToListBox();
                }
                //C:/Work/folder если Слэшей больше одного
                else
                {
                    // показываем путь отняв последний слэш C:/Work/folder -> C:/Work/folder
                    int position = currentPath.LastIndexOf('\\'); // где в последний раз находили слэш - +1 
                    csPathToFolder.PathOfSelectedFolder = null; // очищаем значение
                    csPathToFolder.PathOfSelectedFolder = currentPath.Remove(position);
                    textBlockDirectory.Text = csPathToFolder.PathOfSelectedFolder;
                    GetAddItemsToListBox();
                }

            }

        }

        // Два раза кликаем для того что бы войти в каталог
        private void mouseDoubleClickOnListBox(object sender, MouseButtonEventArgs e)
        {
            if (listBoxDirectories.SelectedItem != null)
            // MessageBox.Show(listBox1.SelectedItem.ToString());
            {

                csPathToFolder.PathOfSelectedFolder = listBoxDirectories.SelectedItem.ToString();
                // Заполняем листбокс найденными каталогами
                GetAddItemsToListBox();
            }
        }

        // Заносим текущее выделение в Путь
        private void currentSelect(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxDirectories.SelectedItem != null)
            // MessageBox.Show(listBox1.SelectedItem.ToString());
            {
                textBlockDirectory.Text = listBoxDirectories.SelectedItem.ToString();
                csPathToFolder.PathOfSelectedFolder = listBoxDirectories.SelectedItem.ToString();

            }
        }
        #region Методы
        // Выводим папки
        private void GetAddItemsToListBox()
        {
            textBlockDirectory.Text = csPathToFolder.PathOfSelectedFolder;
            string[] folderArr = Directory.GetDirectories(csPathToFolder.PathOfSelectedFolder);
            listBoxDirectories.Items.Clear();
            foreach (string folders in folderArr)
            {
                listBoxDirectories.Items.Add(folders);
            }
        }

        // Заполняем Листбокс локальными дисками
        private void GetAddDrivesToListBox()
        {
            string[] myDrives = Environment.GetLogicalDrives();
            csPathToFolder.PathOfSelectedFolder = null;
            textBlockDirectory.Text = csPathToFolder.PathOfSelectedFolder;
            listBoxDirectories.Items.Clear();
            foreach (string myDrive in myDrives)
            {
                if (myDrive != "F:\\")
                {
                    listBoxDirectories.Items.Add(myDrive);
                }
            }
        }
        #endregion






    }
}

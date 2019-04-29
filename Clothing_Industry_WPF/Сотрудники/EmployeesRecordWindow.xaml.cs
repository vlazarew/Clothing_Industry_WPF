using Clothing_Industry_WPF.Перечисления;
using System;
using System.Collections.Generic;
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

namespace Clothing_Industry_WPF.Сотрудники
{
    /// <summary>
    /// Логика взаимодействия для EmployeesRecordWindow.xaml
    /// </summary>
    public partial class EmployeesRecordWindow : Window
    {
        WaysToOpenForm.WaysToOpen way;

        public EmployeesRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen)
        {
            InitializeComponent();
            way = waysToOpen;
            setNewTitle();           
        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";
                    break;
                default:
                    break;
            }
        }
    }
}

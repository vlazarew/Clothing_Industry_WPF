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
using System.Xml.Linq;

namespace Clothing_Industry_WPF.MainForms
{
    /// <summary>
    /// Логика взаимодействия для ParametersWindow.xaml
    /// </summary>
    public partial class ConnectionSettingsWindow : Window
    {
        // Имя xml файла
        private string xmlFileName = "connectionSettings.xml";

        public string Result { get; set; }

        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            textBoxIP.Focus();
            LoadFromXML();
        }

        private void LoadFromXML()
        {
            XDocument xmlDocument = XDocument.Load(xmlFileName);
            var ip = xmlDocument.Element("settings").Element("IpAddress").FirstAttribute;
            textBoxIP.Text = ip.Value;
        }

        private void ButtonSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            XDocument xmlDocument = XDocument.Load(xmlFileName);
            var ip = xmlDocument.Element("settings").Element("IpAddress").FirstAttribute;
            ip.Value = textBoxIP.Text;

            xmlDocument.Save(xmlFileName);
            Result = ip.Value;
            Close();
        }
    }
}

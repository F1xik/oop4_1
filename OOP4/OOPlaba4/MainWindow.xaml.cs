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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using Weapons;


namespace OOPlaba4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Weapon> weapons = new ObservableCollection<Weapon>();
        ObservableCollection<Weapon> weapons1 = new ObservableCollection<Weapon>();
        ObservableCollection<Weapon> weapons2 = new ObservableCollection<Weapon>();
        static JsonSerializer jsonSerializer = new JsonSerializer();
        List<string> classesNames = new List<string>();
        List<Type> dllTypes = new List<Type>();
        string sourcePath = @"C:\Users\Андрей\Desktop\OOPlaba4\OOPlaba4";
        public MainWindow()
        {   
            InitializeComponent();
        }

        class Wrapper
        {
            public ObservableCollection<Weapon> Weapons { get; set; }
        }

        private string GetFileType(string fileName)
        {
            string fileType = "";
            string temp = "";
            int i = fileName.Length - 1;

            while (fileName[i] != '.')
            {
                temp += fileName[i];
                i--;
            }
            for (i = (temp.Length - 1); i >= 0; i--)
            {
                fileType += temp[i];
            }
            return fileType;
        }

        private void CreateFilesPaths()
        {
            List<string> filesPaths = new List<string>(Directory.EnumerateFiles(sourcePath));
            for (int i = 0; i < filesPaths.Count; i++)
            {
                if (GetFileType(filesPaths[i]) == "dll")
                {
                    classesNames.Add(filesPaths[i]);
                }
            }
        }

        private void CreateAllTypes()
        {
            for (int i = 0; i < classesNames.Count; i++)
            {
                Assembly temp = Assembly.LoadFrom(classesNames[i]);
                dllTypes.AddRange(temp.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Weapon))));
            }
        }

        private void CreateAllObjects()
        {
            object newWeapon;
            for (int i = 0; i < dllTypes.Count; i++)
            {
                newWeapon = Activator.CreateInstance(dllTypes[i]);
                weapons1.Add((Weapon)newWeapon);
            }
            newWeapon = new Weapon();
            weapons1.Add((Weapon)newWeapon);
        }

        

        private void ShowAllProperties(ListBox list)
        {
            int i = 0;
            Type type = list.SelectedItem.GetType();
            Fields.Children.Clear();
            foreach (PropertyInfo property in type.GetProperties())
            {
                TextBox textBox = new TextBox();
                Label label = new Label();
                Binding binding = new Binding();
                binding.Source = list.SelectedItem;
                binding.Path = new PropertyPath(property.Name);
                binding.Mode = BindingMode.TwoWay;
                
                label.Content = property.Name;
                textBox.SetBinding(TextBox.TextProperty, binding);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(label, i);
                Grid.SetRow(textBox, i);

                var rowDefinition = new RowDefinition { Height = GridLength.Auto };
                Fields.RowDefinitions.Add(rowDefinition);
                Fields.Children.Add(textBox);
                Fields.Children.Add(label);
                i++;
            }
        }

        private void GetListOfHollows_Click(object sender, RoutedEventArgs e)
        {
            CreateFilesPaths();
            CreateAllTypes();
            CreateAllObjects();
            WeaponsTypes.ItemsSource = weapons1;
            WeaponsList.ItemsSource = weapons;
        }

        private void Serialize()
        {
            FileStream stream;
            int i;
            for (i = 0; i < weapons.Count; i++)
            {
                string fileName = "BsonFile";
                fileName = fileName + i.ToString();
                stream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                using (BsonWriter bsonWriter = new BsonWriter(stream))
                {
                    jsonSerializer.TypeNameHandling = TypeNameHandling.All;
                    jsonSerializer.Serialize(bsonWriter, weapons[i]);
                }
                stream.Close();
            }
        }

        

        private void Serialization_Click(object sender, RoutedEventArgs e)
        {
            FileStream stream;
            stream = File.Open("BsonFile", FileMode.OpenOrCreate, FileAccess.Write);

            using (BsonWriter bsonWriter = new BsonWriter(stream))
            {
                jsonSerializer.TypeNameHandling = TypeNameHandling.All;
                var obj = new Wrapper { Weapons = weapons };                
                jsonSerializer.Serialize(bsonWriter, obj);
            }
            stream.Close();
            

        }

        private void Deserialization_Click(object sender, RoutedEventArgs e)
        {
            FileStream stream;
            stream = File.Open("BsonFile", FileMode.Open, FileAccess.Read);

            using (BsonReader bsonReader = new BsonReader(stream))
            {
                jsonSerializer.TypeNameHandling = TypeNameHandling.All;
                var obj = new Wrapper { Weapons = weapons2 };
                
                obj = jsonSerializer.Deserialize<Wrapper>(bsonReader);
                
                weapons2 = obj.Weapons;
                DsrlzList.ItemsSource = weapons2;
                

            }
            stream.Close();
            
        }

        private void AddToHollowsList_Click(object sender, RoutedEventArgs e)
        {
            Type weaponType = WeaponsTypes.SelectedItem.GetType();
            object newWeapons = Activator.CreateInstance(weaponType);            
            weapons.Add((Weapon)newWeapons);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if(WeaponsList.SelectedValue != null)
                ShowAllProperties(WeaponsList);
            else
            {
                ShowAllProperties(DsrlzList);
            }
        }
    }
}

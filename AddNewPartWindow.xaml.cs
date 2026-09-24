using System;
using System.Collections.Generic;
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
using Microsoft.Win32;

namespace PartsInfo
{
    /// <summary>
    /// Interaction logic for AddNewPartWindow.xaml
    /// </summary>
    public partial class AddNewPartWindow : Window
    {
        List<object> fields;
        PartsDatabaseManager databaseManager;
        MainWindow mainWindow;

        public AddNewPartWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            databaseManager = new PartsDatabaseManager();
            this.mainWindow = mainWindow;
            fields = new List<object>();

            fields.Add(TextboxPartNumber);
            fields.Add(ButtonImage);
            fields.Add(TextboxDrawing);
            fields.Add(TextboxRev);
            fields.Add(TextboxDescription);
            fields.Add(TextboxMaterial);
            fields.Add(TextboxSupplier);
            fields.Add(TextboxMaterialCost);
            fields.Add(TextboxPriceEach);
            fields.Add(ComboboxSource);
            fields.Add(TextboxComment);

            // Combo box initialization
            SetSourceComboBox();
        }

        

        #region // Combo Box Initialization
        private void SetSourceComboBox()
        {
            List<string> options = new List<string>();
            options.Add("");
            options.Add("MEX");
            options.Add("ORA");

            ComboboxSource.ItemsSource = options;
            ComboboxSource.SelectedItem = options[0];
        }

        #endregion

        private Dictionary<string, string> RetrieveFilledFields()
        {
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                if (field is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    fieldValues.Add(textBox.Name.Substring(7), textBox.Text);
                }
                else if (field is ComboBox comboBox && comboBox.SelectedItem.ToString() != "")
                {
                    fieldValues.Add(comboBox.Name.Substring(8), comboBox.SelectedItem.ToString());
                }
                else if (field is Button button && button == ButtonImage && button.Tag is string filePath && !string.IsNullOrWhiteSpace(filePath))
                {
                    fieldValues.Add(button.Name.Substring(6), filePath);
                }
            }

            return fieldValues;
        }

        private void AddImage_Button_Click(object sender, RoutedEventArgs e)
        {
            // Create an OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter for file extension and default file extension
            openFileDialog.Filter = "PNG Files (*.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = openFileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox or process the file
            if (result == true)
            {
                // Open document
                string fullFilePath = openFileDialog.FileName;

                // Set the button content to the file name (display name)
                ButtonImage.Content = System.IO.Path.GetFileName(fullFilePath);

                // Store the full file path in the Tag property of the button
                ButtonImage.Tag = fullFilePath;

                // Optionally show a message or handle the file as needed
                MessageBox.Show($"Selected file: {fullFilePath}");
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            // Checks if part number is filled out.
            if (string.IsNullOrWhiteSpace(TextboxPartNumber.Text))
            {
                MessageBox.Show("Part Number is required!");
            }
            // Checks if drawing is filled out.
            else if (string.IsNullOrWhiteSpace(TextboxDrawing.Text))
            {
                MessageBox.Show("Drawing is required!");
            }
            else 
            {
                if (mainWindow.AddNewPartAndUpdate(RetrieveFilledFields()))
                {
                    Close();
                    MessageBox.Show("New part was added to database!");
                }
            }
        }
    }
}

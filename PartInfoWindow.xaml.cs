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
    /// Interaction logic for PartInfoWindow.xaml
    /// </summary>
    public partial class PartInfoWindow : Window
    {
        MainWindow mainWindow;
        PartInfoModel partInfoObject;

        List<TextBlock> listOfTextblocks;
        List<TextBox> listOfTextboxes;

        List<StackPanel> listOfEditStackPanels;
        List<StackPanel> listOfInfoStackPanels;

        public PartInfoWindow(MainWindow mainWindow, PartInfoModel partInfoObject)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;

            listOfEditStackPanels = new List<StackPanel>();
            listOfInfoStackPanels = new List<StackPanel>();
            InitializeStackPanelEditList();
            InitializeStackPanelInfoList();

            listOfTextblocks = new List<TextBlock>();
            listOfTextboxes = new List<TextBox>();
            InitializeTextBlockList();
            InitializeTextBoxList();

            this.partInfoObject = partInfoObject;
            ShowInfo();
        }

        public void ShowInfo()
        {
            ButtonEdit.Visibility = Visibility.Visible;
            StackpanelEditButtons.Visibility = Visibility.Collapsed;

            // Turn on visibility for textbox
            foreach (var stackEdit in listOfEditStackPanels)
            {
                stackEdit.Visibility = Visibility.Collapsed;
            }
            ButtonImagePath.Visibility = Visibility.Collapsed;

            // Turn off visibility for textblock
            foreach (var stackInfo in listOfInfoStackPanels)
            {
                stackInfo.Visibility = Visibility.Visible;
            }
            ImagePath.Visibility = Visibility.Visible;

            InitializeInfoFields();
        }

        private void UpdatePartAndShowInfo(string partNumber, string drawing)
        {
            partInfoObject = mainWindow.SelectPartObject(partNumber, drawing);

            ShowInfo();
        }

        public void DeletePart()
        {
            MessageBox.Show("Deleting!");
            Close();

            // Executes mysql query and updates database in main window
            mainWindow.DeletePartAndUpdate(TextblockPartNumber.Text, TextblockDrawing.Text);

            InitializeTextBlockList();
            InitializeTextBoxList();
        }

        private void InitializeInfoFields()
        {
            ImagePath.Source = partInfoObject.Image;
            TextblockPartNumber.Text = partInfoObject.PartNumber;
            TextblockDrawing.Text = partInfoObject.Drawing;
            TextblockRev.Text = partInfoObject.Rev;
            TextblockDescription.Text = partInfoObject.Description;
            TextblockMaterial.Text = partInfoObject.Material;
            TextblockSupplier.Text = partInfoObject.Supplier;
            TextblockMaterialCost.Text = "$" + partInfoObject.MaterialCost.ToString();
            TextblockPriceEach.Text = "$" + partInfoObject.PriceEach.ToString();
            TextblockSource.Text = partInfoObject.Source;
            TextblockComment.Text = partInfoObject.Comment;
            ImagePath.Tag = partInfoObject.ImagePath;
        }


        

        #region // Edit mode

        // Hides info stacks/textblocks and reveals edit stacks/textboxes
        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Visibility = Visibility.Collapsed;
            StackpanelEditButtons.Visibility = Visibility.Visible;

            // Turn on visibility for textbox
            foreach (var stackEdit in listOfEditStackPanels)
            {
                stackEdit.Visibility = Visibility.Visible;
            }
            ButtonImagePath.Visibility = Visibility.Visible;

            // Turn off visibility for textblock
            foreach (var stackInfo in listOfInfoStackPanels)
            {
                stackInfo.Visibility = Visibility.Collapsed;
            }
            ImagePath.Visibility = Visibility.Collapsed;

            InitializeEditModeFields();
        }

        // Initializing text boxes to text block values
        private void InitializeEditModeFields()
        {
            for (int i=0; i < listOfTextboxes.Count; i++)
            {
                listOfTextboxes[i].Text = listOfTextblocks[i].Text;
                // If the string is a price, remove the $ when adding value to the textbox
                if (!string.IsNullOrEmpty(listOfTextblocks[i].Text))
                {
                    if (listOfTextblocks[i].Text[0] == '$')
                    {
                        listOfTextboxes[i].Text = listOfTextblocks[i].Text.Substring(1);
                    }
                }
            }
            ButtonImage.Source = ImagePath.Source;
            ButtonImage.Tag = null;
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            PartDeleteConfirmationWindow deleteConfirmationWindow = new PartDeleteConfirmationWindow(this);
            deleteConfirmationWindow.ShowDialog();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            // Checks for unsaved changes and confirms that the user still wants to proceed with exitting edit mode.
            if (ContainsChanges())
            {
                PartEditCancellationWindow editCancellationWindow = new PartEditCancellationWindow(this);
                editCancellationWindow.ShowDialog();
            }
            else
            {
                ShowInfo();
            }
        }

        //Unfinished
        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ContainsChanges())
            {
                MessageBox.Show("No changes were made!");
                return;
            }

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
                Dictionary<string, string> boxFields = RetrieveTextBoxFieldsAsString();
                if (mainWindow.EditPartAndUpdate(TextblockPartNumber.Text, TextblockDrawing.Text, boxFields))
                {
                    MessageBox.Show("Changes have been made!");
                    UpdatePartAndShowInfo(boxFields["PartNumber"], boxFields["Drawing"]);
                }
            }
        }

        // Handles changes made to the image
        private void Edit_Image_Button_Click(object sender, RoutedEventArgs e)
        {
            // Create an OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter for file extension and default file extension
            openFileDialog.Filter = "PNG Files (*.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = openFileDialog.ShowDialog();

            // Check if the user selected a file (result is true)
            if (result == true)
            {
                // Get the selected file name
                string fullFilePath = openFileDialog.FileName;

                // Create image from bitmap
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();

                // Set button to new image and save file path to tag
                ButtonImage.Source = bitmap;
                ButtonImage.Tag = fullFilePath;

                // Show the selected file path in a message box
                MessageBox.Show($"Selected file: {fullFilePath}");
            }
        }

        // Checks if user made changes during edit mode
        private bool ContainsChanges()
        {
            for (int i=0; i < listOfTextboxes.Count; ++i)
            {
                string textBlockValue = listOfTextblocks[i].Text;

                if (!string.IsNullOrEmpty(textBlockValue) && textBlockValue[0] == '$')
                {
                    textBlockValue = textBlockValue.Substring(1);
                }

                if (listOfTextboxes[i].Text != textBlockValue)
                {
                    return true;
                }
            }

            return ButtonImage.Tag is string filePath && !string.IsNullOrWhiteSpace(filePath);
        }
        #endregion

        #region // Lists Initialization
        // Adding stack panels for edit into a list
        private void InitializeStackPanelEditList()
        {
            listOfEditStackPanels.Add(StackpanelEditRow0);
            listOfEditStackPanels.Add(StackpanelEditRow1);
            listOfEditStackPanels.Add(StackpanelEditRow2);
            listOfEditStackPanels.Add(StackpanelEditRow3);
        }

        // Adding stack panels for info into a list
        private void InitializeStackPanelInfoList()
        {
            listOfInfoStackPanels.Add(StackpanelInfoRow0);
            listOfInfoStackPanels.Add(StackpanelInfoRow1);
            listOfInfoStackPanels.Add(StackpanelInfoRow2);
            listOfInfoStackPanels.Add(StackpanelInfoRow3);
        }

        // Adding text blocks into a list
        private void InitializeTextBlockList()
        {
            listOfTextblocks.Add(TextblockPartNumber);
            listOfTextblocks.Add(TextblockDrawing);
            listOfTextblocks.Add(TextblockRev);
            listOfTextblocks.Add(TextblockDescription);
            listOfTextblocks.Add(TextblockMaterial);
            listOfTextblocks.Add(TextblockSupplier);
            listOfTextblocks.Add(TextblockMaterialCost);
            listOfTextblocks.Add(TextblockPriceEach);
            listOfTextblocks.Add(TextblockSource);
            listOfTextblocks.Add(TextblockComment);
        }

        // Adding text box into a list
        private void InitializeTextBoxList()
        {
            listOfTextboxes.Add(TextboxPartNumber);
            listOfTextboxes.Add(TextboxDrawing);
            listOfTextboxes.Add(TextboxRev);
            listOfTextboxes.Add(TextboxDescription);
            listOfTextboxes.Add(TextboxMaterial);
            listOfTextboxes.Add(TextboxSupplier);
            listOfTextboxes.Add(TextboxMaterialCost);
            listOfTextboxes.Add(TextboxPriceEach);
            listOfTextboxes.Add(TextboxSource);
            listOfTextboxes.Add(TextboxComment);
        }

        private Dictionary<string, string> RetrieveTextBoxFieldsAsString()
        {
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();

            foreach (var textBox in listOfTextboxes)
            {
                fieldValues.Add(textBox.Name.Substring(7), textBox.Text);
            }

            if (ButtonImage.Tag is string filePath && !string.IsNullOrWhiteSpace(filePath))
            {
                fieldValues.Add("Image", filePath);
            }

            return fieldValues;
        }
        #endregion
    }
}

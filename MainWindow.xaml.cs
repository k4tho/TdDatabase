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
using System.Configuration;
using PartsInfo;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PartsInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SearchByMenuOptions> searchOptions;
        public ObservableCollection<PartInfoModel> PartsList { get; set; }
        private PartsDatabaseManager databaseManager;

        public MainWindow()
        {
            InitializeComponent();

            searchOptions = new List<SearchByMenuOptions>();
            searchOptions.Add(new SearchByMenuOptions { property = "Drawing Number" });
            searchOptions.Add(new SearchByMenuOptions { property = "Part Number" });
            searchOptions.Add(new SearchByMenuOptions { property = "Description" });

            SearchByMenu.ItemsSource = searchOptions;
            SearchByMenu.SelectedItem = searchOptions[1];

            try
            {
                databaseManager = new PartsDatabaseManager();
                SetPartsDatabase();
            }
            catch (InvalidOperationException exception)
            {
                MessageBox.Show(exception.Message, "Database setup required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
        }

        private void PartsNavigationButton_Click(object sender, RoutedEventArgs e)
        {
            PartsView.Visibility = Visibility.Visible;
            OrdersView.Visibility = Visibility.Collapsed;
            PartsNavigationButton.Background = (Brush)new BrushConverter().ConvertFrom("#2B526F");
            OrdersNavigationButton.Background = Brushes.Transparent;
        }

        private void OrdersNavigationButton_Click(object sender, RoutedEventArgs e)
        {
            PartsView.Visibility = Visibility.Collapsed;
            OrdersView.Visibility = Visibility.Visible;
            PartsNavigationButton.Background = Brushes.Transparent;
            OrdersNavigationButton.Background = (Brush)new BrushConverter().ConvertFrom("#2B526F");
        }


        #region // Search Engine

        private void PerformSearch()
        {
            // Variable initialization
            string searchBoxInput = SearchPartTextBox.Text;
            List<PartInfoModel> results;

            // Checks if search input is null
            if (string.IsNullOrWhiteSpace(searchBoxInput))
            {
                searchBoxInput = "";
            }

            PartsList.Clear();
            // Search by three different attributes
            if (SearchByMenu.SelectedItem == searchOptions[0])
            {
                results = databaseManager.ExecuteRetrievalOfObjects(databaseManager.SearchByDrawing(searchBoxInput));
            }
            else if (SearchByMenu.SelectedItem == searchOptions[1])
            {
                results = databaseManager.ExecuteRetrievalOfObjects(databaseManager.SearchByPartNumber(searchBoxInput));
            }
            else
            {
                results = databaseManager.ExecuteRetrievalOfObjects(databaseManager.SearchByDescription(searchBoxInput));
            }

            // Compiles results into list
            foreach (var part in results)
            {
                PartsList.Add(part);
            }
        }

        // Search by button click
        private void SearchPartButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        // Search by pressing enter
        private void SearchPartTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
                e.Handled = true; // Prevents the Enter key from being processed further
            }
        }
        #endregion

        #region // Database

        // Database Initialization
        private void SetPartsDatabase()
        {
            PartsList = new ObservableCollection<PartInfoModel>(databaseManager.ExecuteRetrievalOfObjects(databaseManager.SelectAllData()));
            PartsDatabase.Items.Clear();
            PartsDatabase.ItemsSource = PartsList;
        }

        // Click on part number for more details
        private void PartNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string partNumber = button.Content.ToString();

            // Getting drawing number associated with the button's row
            var rowData = button.DataContext as PartInfoModel;
            string drawing = rowData.Drawing;

            PartInfoModel selectedPartObject = SelectPartObject(partNumber, drawing);

            // Open new window for part description
            PartInfoWindow partInfoWindow = new PartInfoWindow(this, selectedPartObject);
            partInfoWindow.Show();
        }

        // Get PartInfoModel object based on partnumber and drawing
        public PartInfoModel SelectPartObject(string partNumber, string drawing)
        {
            PartInfoModel selectedPartObject = databaseManager.ExecuteObjectRetrievalByPartNumberAndDrawing(partNumber, drawing);
            return selectedPartObject;
        }
        #endregion

        #region // Add Delete Parts

        public bool AddNewPartAndUpdate(Dictionary<string, string> fieldValues)
        {
            if (databaseManager.ExecutePartAddition(fieldValues))
            {
                RefreshDatabase();
                return true;
            }
            return false;
        }

        public bool EditPartAndUpdate(string partNumber, string drawing, Dictionary<string, string> fieldValues)
        {
            if (databaseManager.ExecutePartAlteration(partNumber, drawing, fieldValues))
            {
                RefreshDatabase();
                return true;
            }
            return false;
        }

        public void DeletePartAndUpdate(string partNumber, string drawing)
        {
            databaseManager.ExecutePartDeletion(partNumber, drawing);

            RefreshDatabase();
        }

        private void AddPart_Button_Click(object sender, RoutedEventArgs e)
        {
            AddNewPartWindow addNewPartWindow = new AddNewPartWindow(this);
            addNewPartWindow.ShowDialog();
        }

        private void RefreshDatabase()
        {
            List<PartInfoModel> results = databaseManager.ExecuteRetrievalOfObjects(databaseManager.SelectAllData());

            // Clears current database and refreshes it
            PartsList.Clear();
            foreach (var part in results)
            {
                PartsList.Add(part);
            }
        }

        #endregion
    }
}

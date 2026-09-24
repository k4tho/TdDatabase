using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace PartsInfo
{
    class PartsDatabaseManager
    {
        private MySqlConnection connection;
        private string connectionString;

        private List<PartInfoModel> partsList;

        private Dictionary<string, int> fieldSizes;         // To check if size of var will fit in database

        // Constructor
        public PartsDatabaseManager()
        {
            ConnectToDatabase();
            InitializeFieldSizes();
        }

        private void InitializeFieldSizes()
        {
            fieldSizes = new Dictionary<string, int>();

            fieldSizes.Add("PartNumber", 25);
            fieldSizes.Add("Drawing", 25);
            fieldSizes.Add("Rev", 10);
            fieldSizes.Add("Description", 150);
            fieldSizes.Add("Material", 100);
            fieldSizes.Add("Supplier", 25);
            fieldSizes.Add("MaterialCost", 10);
            fieldSizes.Add("PriceEach", 10);
            fieldSizes.Add("Source", 10);
            fieldSizes.Add("Comment", 255);
        }

        private void ConnectToDatabase()
        {
            string databaseUser = Environment.GetEnvironmentVariable("TD_DATABASE_USER");
            string databasePassword = Environment.GetEnvironmentVariable("TD_DATABASE_PASSWORD");

            if (string.IsNullOrWhiteSpace(databaseUser) || string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new InvalidOperationException(
                    "Database credentials are missing. Set the TD_DATABASE_USER and " +
                    "TD_DATABASE_PASSWORD Windows environment variables, then restart the application.");
            }

            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = "localhost",
                Database = "partinfo",
                UserID = databaseUser,
                Password = databasePassword
            };

            connectionString = connectionStringBuilder.ConnectionString;
            connection = new MySqlConnection(connectionString);
        }

        #region // Query execution

        public List<PartInfoModel> ExecuteRetrievalOfObjects(MySqlCommand cmd)
        {
            partsList = new List<PartInfoModel>();

            try
            {
                connection.Open();
                cmd.Connection = connection;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PartInfoModel part = new PartInfoModel
                        {
                            PartNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Drawing = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Rev = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Material = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Supplier = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            MaterialCost = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                            PriceEach = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                            Source = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                            Comment = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        };
                        // Retrieve the image (if stored as BLOB)
                        if (!reader.IsDBNull(0))
                        {
                            byte[] imageData = (byte[])reader["Image"];
                            if (imageData != null && imageData.Length > 0)
                            {
                                // Convert byte array to BitmapSource
                                using (var stream = new MemoryStream(imageData))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.StreamSource = stream;
                                    bitmap.EndInit();
                                    part.Image = bitmap; // Set the Image property
                                }
                            }
                        }
                        partsList.Add(part);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return partsList;
        }

        public PartInfoModel ExecuteObjectRetrievalByPartNumberAndDrawing(string partNumber, string drawing)
        {
            MySqlCommand cmd = FindExactPart(partNumber, drawing);

            PartInfoModel part = new PartInfoModel();
            try
            {
                connection.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        part = new PartInfoModel
                        {
                            PartNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Drawing = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Rev = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Material = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Supplier = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            MaterialCost = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                            PriceEach = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                            Source = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                            Comment = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        };
                        // Retrieve the image (if stored as BLOB)
                        if (!reader.IsDBNull(0))
                        {
                            byte[] imageData = (byte[])reader["Image"];
                            if (imageData != null && imageData.Length > 0)
                            {
                                // Convert byte array to BitmapSource
                                using (var stream = new MemoryStream(imageData))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.StreamSource = stream;
                                    bitmap.EndInit();
                                    part.Image = bitmap; // Set the Image property
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return part;
        }

        public bool ExecutePartAddition(Dictionary<string, string> fieldValues)
        {
            Dictionary<string, object> updatedFieldValues = ConvertToCorrectVarTypes(fieldValues);

            //Checks for duplicate primary key
            if (HasDuplicatePrimaryKey(fieldValues["PartNumber"], fieldValues["Drawing"]))
            {
                MessageBox.Show("Duplicate key found! Query could not be executed. Please enter a different part or drawing number.");
                return false;
            }
            // Checks variable size and conversion from string to decimal/int
            if (!ContainsValidInputs(updatedFieldValues))
            {
                return false;
            }

            MySqlCommand cmd = Insert(updatedFieldValues);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return true;
        }

        public void ExecutePartDeletion(string partNumber, string drawing)
        {
            MySqlCommand cmd = Delete(partNumber, drawing);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        public bool ExecutePartAlteration(string partNumber, string drawing, Dictionary<string, string> fieldValues)
        {
            Dictionary<string, object> updatedFieldValues = ConvertToCorrectVarTypes(fieldValues);

            // Checks variable size, conversion from string to decimal/int
            if (!ContainsValidInputs(updatedFieldValues))
            {
                return false;
            }


            // If the key changed, make sure the new key is not already used.
            if (((partNumber != fieldValues["PartNumber"]) || (drawing != fieldValues["Drawing"])) &&
                HasDuplicatePrimaryKey(fieldValues["PartNumber"], fieldValues["Drawing"]))
            {
                MessageBox.Show("Part and drawing number already exists in the database. Please change it.");
                return false;
            }

            MySqlCommand cmd = Update(partNumber, drawing, updatedFieldValues);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return true;
        }

        #endregion

        #region // Retrieving mysql queries
        public MySqlCommand SelectAllData()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM partinfo.parts;", connection);

            return cmd;
        }

        public MySqlCommand SearchByPartNumber(string input)
        {
            // Create the query
            string query = "SELECT * FROM partinfo.parts WHERE PartNumber LIKE CONCAT(@PartNumber, '%')";

            // Create a MySqlCommand and add the parameter
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@PartNumber", input);

            return cmd;
        }

        public MySqlCommand SearchByDrawing(string input)
        {
            string query = "SELECT * FROM partinfo.parts WHERE Drawing LIKE CONCAT(@Drawing, '%')";

            // Create a MySqlCommand and add the parameter
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@Drawing", input);

            return cmd;
        }

        public MySqlCommand SearchByDescription(string input)
        {
            string query = "SELECT * FROM partinfo.parts WHERE Description LIKE CONCAT(@Description, '%')";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Description", input);

            return cmd;
        }

        private MySqlCommand FindExactPart(string partNumberInput, string drawingInput)
        {
            string query = "SELECT * FROM partinfo.parts WHERE PartNumber = @partNumber AND Drawing = @drawing";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@partNumber", partNumberInput);
            cmd.Parameters.AddWithValue("@drawing", drawingInput);

            return cmd;
        }

        private MySqlCommand Insert(Dictionary<string, object> fieldValues)
        {
            if (fieldValues.Count == 0)
            {
                MessageBox.Show("Nothing to add!");
                return null;
            }

            string query = "INSERT INTO partinfo.parts ({columns}) VALUES ({parameters})";

            var columns = string.Join(", ", fieldValues.Keys);
            var parameters = string.Join(", ", fieldValues.Keys.Select(key => "@" + key));

            // Construct the full query string
            query = query.Replace("{columns}", columns).Replace("{parameters}", parameters);

            MySqlCommand cmd = new MySqlCommand(query, connection);

            // Add each parameter to the MySqlCommand
            foreach (var fieldValue in fieldValues)
            {
                if (fieldValue.Key == "Image")
                {
                    cmd.Parameters.Add("@" + fieldValue.Key, MySqlDbType.LongBlob).Value = fieldValue.Value;
                }
                else
                {
                    // Add the parameter with its corresponding value
                    cmd.Parameters.AddWithValue("@" + fieldValue.Key, fieldValue.Value);
                }
            }

            return cmd;
        }

        private MySqlCommand Delete(string partNumberInput, string drawingInput)
        {
            string query = "DELETE FROM partinfo.parts WHERE PartNumber = @partNumber AND Drawing = @drawing";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@partNumber", partNumberInput);
            cmd.Parameters.AddWithValue("@drawing", drawingInput);

            return cmd;
        }

        private MySqlCommand Update(string partNumber, string drawing, Dictionary<string, object> fieldValues)
        {
            string imageUpdate = fieldValues.ContainsKey("Image") ? "Image = @image, " : "";
            string query = "UPDATE partinfo.parts SET " + imageUpdate + "PartNumber = @updatedPartNumber, Drawing = @updatedDrawing, Rev = @rev, Description = @description, Material = @material, Supplier = @supplier, MaterialCost = @materialCost, PriceEach = @priceEach, Source = @source, Comment = @comment WHERE PartNumber = @partNumber AND Drawing = @drawing";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            if (fieldValues.ContainsKey("Image"))
            {
                cmd.Parameters.Add("@image", MySqlDbType.LongBlob).Value = fieldValues["Image"];
            }

            cmd.Parameters.AddWithValue("@partNumber", partNumber);
            cmd.Parameters.AddWithValue("@drawing", drawing);
            cmd.Parameters.AddWithValue("@updatedPartNumber", fieldValues["PartNumber"]);
            cmd.Parameters.AddWithValue("@updatedDrawing", fieldValues["Drawing"]);
            cmd.Parameters.AddWithValue("@rev", fieldValues["Rev"]);
            cmd.Parameters.AddWithValue("@description", fieldValues["Description"]);
            cmd.Parameters.AddWithValue("@material", fieldValues["Material"]);
            cmd.Parameters.AddWithValue("@supplier", fieldValues["Supplier"]);
            cmd.Parameters.AddWithValue("@materialCost", fieldValues["MaterialCost"]);
            cmd.Parameters.AddWithValue("@priceEach", fieldValues["PriceEach"]);
            cmd.Parameters.AddWithValue("@source", fieldValues["Source"]);
            cmd.Parameters.AddWithValue("@comment", fieldValues["Comment"]);

            return cmd;
        }

        #endregion

        #region // Extra

        private bool ContainsValidInputs(Dictionary<string, object> fieldValues)
        {
            // If field conversions for decimals and integers were unsuccessful.
            if (fieldValues.Count == 0)
            {
                return false;
            }

            // If variable sizes are too large for database.
            if (!CheckVarSizes(fieldValues))
            {
                return false;
            }

            return true;
        }

        public bool HasDuplicatePrimaryKey(string partNumber, string drawing)
        {
            MySqlCommand cmd = FindExactPart(partNumber, drawing);

            try
            {
                connection.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private Dictionary<string, object> ConvertToCorrectVarTypes(Dictionary<string, string> fieldValues)
        {
            Dictionary<string, object> updatedFieldValues = new Dictionary<string, object>();

            foreach (var field in fieldValues)
            {
                // Removes extra whitespaces from variables
                object value = field.Value.Trim();

                if (string.IsNullOrEmpty(field.Value.Trim()))
                {
                    value = null;
                }
                else if (field.Key == "Image")
                {
                    try
                    {
                        value = File.ReadAllBytes(field.Value.Trim());
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The selected image could not be read.");
                        return new Dictionary<string, object>();
                    }
                }
                else if (field.Key == "MaterialCost")
                {
                    if (decimal.TryParse(field.Value.Trim(), out decimal materialCost))
                    {
                        value = materialCost;
                    }
                    else
                    {
                        MessageBox.Show("Material Cost must be a decimal!");
                        return new Dictionary<string, object>();
                    }
                }
                else if (field.Key == "PriceEach")
                {
                    if (decimal.TryParse(field.Value.Trim(), out decimal priceEach))
                    {
                        value = priceEach;
                    }
                    else
                    {
                        MessageBox.Show("Price Each must be a decimal!");
                        return new Dictionary<string, object>();
                    }
                }
                updatedFieldValues.Add(field.Key, value);
            }

            return updatedFieldValues;
        }

        private bool CheckVarSizes(Dictionary<string, object> fieldValues)
        {
            foreach (var field in fieldValues)
            {
                if (field.Key == "PartNumber" || field.Key == "Drawing" || field.Key == "Rev" || field.Key == "Description" || field.Key == "Material" || field.Key == "Supplier" || field.Key == "Source" || field.Key == "Comment")
                {
                    // Cast the value to a string and check its length
                    string value = fieldValues[field.Key] as string;

                    if (value != null && value.Length > fieldSizes[field.Key])
                    {
                        MessageBox.Show($"Value of {field.Key} must be less than {fieldSizes[field.Key]} characters!");
                        return false;
                    }
                }
            }

            return true;
        }
    }
    #endregion
}

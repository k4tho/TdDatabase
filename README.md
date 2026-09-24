# Parts Inventory Manager

A Windows desktop application for organizing, searching, and maintaining part records in a MySQL database. The application provides a clean catalog interface for viewing part details, costs, suppliers, drawings, and images.

The project also includes an Orders interface prototype. Its database functionality has not yet been implemented.

## Features

- Browse parts in a searchable catalog
- Search by:
  - Part number
  - Drawing number
  - Description
- Add new parts
- Edit existing part records
- Delete parts with confirmation
- Store and display part images
- Track:
  - Drawing and revision numbers
  - Materials
  - Suppliers
  - Material costs
  - Unit prices
  - Sources
  - Comments
- Orders interface with:
  - Order ID, customer name, and date search options
  - Earliest-to-latest and latest-to-earliest sorting
  - Add Customer button
- Tracking:
  - Order ID
  - Customer Name
  - Order Date
  - Shipment Status
  - Products Ordered

## Screenshots

- [Parts Catalog](SamplePhotos/PartsCatalog.png)
- [Part Details](SamplePhotos/PartDetails.png)
- [Add Part](SamplePhotos/AddPart.png)
- [Edit Part](SamplePhotos/EditPart.png)
- [Orders Catalog](SamplePhotos/OrdersCatalog.png)

## Technology

- C#
- WPF
- .NET Framework 4.7.2
- MySQL
- MySql.Data
- Visual Studio 2022

## Requirements

Before running the application, install:

- Visual Studio 2022 with the **.NET desktop development** workload
- .NET Framework 4.7.2
- MySQL Server 8
- NuGet package restore support in Visual Studio

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/k4tho/TdDatabase.git
cd TdDatabase
```

### 2. Open the solution

Open `PartsInfo.sln` in Visual Studio.

If Visual Studio reports missing packages, right-click the solution and select **Restore NuGet Packages**.

### 3. Create the database

Sign in to your local MySQL server and run:

```sql
CREATE DATABASE partinfo;

USE partinfo;

CREATE TABLE parts (
    Image LONGBLOB NULL,
    PartNumber VARCHAR(25) NOT NULL,
    Drawing VARCHAR(25) NOT NULL,
    Rev VARCHAR(10),
    Description VARCHAR(150),
    Material VARCHAR(100),
    Supplier VARCHAR(25),
    MaterialCost DECIMAL(10, 2),
    PriceEach DECIMAL(10, 2),
    Source VARCHAR(10),
    Comment VARCHAR(255),
    PRIMARY KEY (PartNumber, Drawing)
);
```

The column order should remain the same because the current retrieval code expects this structure.

### 4. Create a database user

Create a dedicated account instead of running the application with the MySQL root account:

```sql
CREATE USER 'td_app'@'localhost'
IDENTIFIED BY 'choose-a-strong-password';

GRANT SELECT, INSERT, UPDATE, DELETE
ON partinfo.*
TO 'td_app'@'localhost';
```

Replace `choose-a-strong-password` with your own password.

### 5. Configure your credentials

Database credentials are read from Windows environment variables and are not stored in the source code.

Open **Edit environment variables for your account** from the Windows Start menu. Under **User variables**, create:

```text
TD_DATABASE_USER = td_app
TD_DATABASE_PASSWORD = your-database-password
```

Restart Visual Studio after adding or changing these variables.

### 6. Run the application

In Visual Studio:

1. Set `PartsInfo` as the startup project.
2. Build the solution.
3. Press `F5` to run it.

MySQL must be running before the application starts.

## Project Structure

```text
PartsInfo/
├── MainWindow.xaml
├── PartsDatabaseManager.cs
├── PartInfoModel.cs
├── PartInfoWindow.xaml
├── AddNewPartWindow.xaml
├── Preprocessing/
├── Properties/
└── PartsInfo.sln
```

## Privacy and Sample Data

This repository intentionally excludes:

- Database credentials
- Company database exports
- Real part records
- Part images
- CSV and Excel source data
- Visual Studio temporary files
- Compiled application files

The `Data` and `Images` directories are excluded through `.gitignore`.

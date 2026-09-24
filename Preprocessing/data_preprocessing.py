"""
Overview
------
This script preprocesses an xlsx file and adds it to the mysql database.

Key Variables To Edit
-------
    * host_input (str): host for mysql connection
    * user_input (str): username for mysql connection
    * password_input (str): password for mysql connection
    * database_input (str): database table name
"""

import pandas as pd
import numpy as np
import os
import pymysql
from openpyxl import load_workbook


def preprocess_decimals(decimal_val):
    # If alr float, return it. It should be a nan value
    if isinstance(decimal_val, float):
        return decimal_val
    elif isinstance(decimal_val, int):
        return float(decimal_val)
    else:
        # Remove whitespaces and $ in string
        new_decimal_val = decimal_val.replace(" ", "").replace("$", "")
        # If value is -, it will return nan
        if new_decimal_val[0] == '-':
            return np.nan
        # Convert to float unless there are some edge cases. Return float or nan
        try: 
            float(new_decimal_val)
            return float(new_decimal_val)
        except ValueError:
            return np.nan

# Ensure consistency. No additional whitespace in front or end of column name
def rename_columns(df):
    for column in df.columns:
        if column[0] == ' ':
            df.rename(columns={column: column[1:]}, inplace=True)
    for column in df.columns:
        if column[-1] == ' ':
            df.rename(columns={column: column[:-1]}, inplace=True)

def preprocess_df(df):
    df.drop('Num', axis=1, inplace=True)
    rename_columns(df)

    df['COST MATERIAL'] = df['COST MATERIAL'].apply(preprocess_decimals)
    df['PRICE EACH\nOR SET'] = df['PRICE EACH\nOR SET'].apply(preprocess_decimals)

    df = df.replace({np.nan: None})

    return df








host_input, user_input, password_input, database_input = "", "", "", ""
data_path = os.path.join('..', 'Data', 'part_info_nov_05-28-22.xlsx')

# Load the Excel file into a DataFrame
df = pd.read_excel(data_path, engine='openpyxl')
df = preprocess_df(df)

# Load the Excel file for image extraction
wb = load_workbook(data_path)
sheet = wb.active

# Extract images and tracks it's associated row from the Excel sheet
images, indices = [], []
for i, img in enumerate(sheet._images):
    indices.append(img.anchor._from.row-1)
    images.append(img._data())  # Image binary data

db_connection = pymysql.connect(
    host=host_input,
    user=user_input,
    password=password_input,
    database=database_input
)
cursor = db_connection.cursor()

# Insert data into the MySQL table
for index, row in df.iterrows():
    # Get text data from the DataFrame
    part_number = row['PART NUMBER']
    drawing = row['DRAWING']
    rev = row['REV']
    description = row['DESCRIPTION']
    material = row['MATERIAL']
    supplier = row['SUPPLIER']
    material_cost = row['COST MATERIAL']
    price_each = row['PRICE EACH\nOR SET']
    source = row['SOURCE']

    # Get image data for this row if it exists
    if index in indices:
        image_index = indices.index(index)
        image = images[image_index]
    else:
        image = None

    # Insert the row into the database
    query = "INSERT IGNORE INTO parts (Image, PartNumber, Drawing, Rev, Description, Material, Supplier, MaterialCost, PriceEach, Source) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
    cursor.execute(query, (image, part_number, drawing, rev, description, material, supplier, material_cost, price_each, source))

# Commit the transaction and close the connection
db_connection.commit()
cursor.close()
db_connection.close()

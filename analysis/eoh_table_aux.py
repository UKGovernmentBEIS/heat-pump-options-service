import numpy as np
import os
import pandas as pd
import requests
import json
from typing import Tuple


def get_survey_datasets() -> Tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]:
    """ " Get the datasets from USMART.io

    Returns:
        Tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]: The required datasets: Survey, Heat pump and EPC datasets.
    """

    def create_dataset(url: str) -> pd.DataFrame:
        """Get a single dataset from USMART.io

        Args:
            url (str): URL for the required dataset

        Returns:
            pd.DataFrame: The required dataset
        """
        use_url = pre_ + "/" + url + "/latest/urql"
        response = requests.request("GET", use_url, headers=headers)
        type(response)
        file = json.loads(response.text)
        df = pd.DataFrame(file)
        return df

    pre_ = "https://api.usmart.io/org"
    headers = {
        "cache-control": "no-cache",
        "api-key-id": os.environ["USMART_KEY_ID"],
        "api-key-secret": os.environ["USMART_KEY_SECRET"],
    }
    urls_ = pd.read_json("urls_v2.json")  # version 2 updated on 1/12/2021
    dfs_survey = []
    dfs_hp = []
    dfs_epc = []
    for surveyor in ["ovo", "warmworks", "e.on"]:
        print("working with data from surveyor:", surveyor)
        df_survey = create_dataset(urls_[surveyor].values[2])
        # Remove cases where the head load is not present in the data
        # CODE REVIEW - DESIGN: How many cases are there of this? Does it influence the results compared to the categorical modelling (i.e. are they missing not at random)?
        df_survey = df_survey.loc[df_survey["MCS_SHLoad"] != "", :]
        dfs_survey.append(df_survey)
        df_hp = create_dataset(urls_[surveyor].values[3])
        df_hp["surveyor"] = surveyor
        try:
            df_hp["Cost_HP"] = df_hp["Cost_HP"].str.replace(",", "")
            df_hp["Cost_HP"] = df_hp["Cost_HP"].str.replace("�", "").astype(float)
            df_hp["Cost_Install"] = df_hp["Cost_Install"].replace("N/A", np.nan)
            df_hp["Cost_Install"] = df_hp["Cost_Install"].replace("n/a", np.nan)
            df_hp["Cost_Install"] = df_hp["Cost_Install"].replace("n/A", np.nan).astype(float)
        except AttributeError:
            pass
        dfs_hp.append(df_hp)
        dfs_epc.append(create_dataset(urls_[surveyor].values[1]))
    df_survey = pd.concat(dfs_survey)
    df_hp = pd.concat(dfs_hp)
    df_epc = pd.concat(dfs_epc)
    return df_survey, df_hp, df_epc


def replace_vars(df: pd.DataFrame) -> pd.DataFrame:
    """Variables need be in a standardised form, we do the replacement here

    Args:
        df (pd.DataFrame): Dataframe with variables to be replaced

    Returns:
        pd.DataFrame: Dataframe with replaced variables
    """
    #
    df.loc[df.House_Form == "Mid - Terrace", "House_Form"] = "Mid-Terrace"
    df.loc[df.House_Form == "End - Terrace", "House_Form"] = "Semi-Detached"  #'End-Terrace'
    df.loc[df.House_Form == "End -Terrace", "House_Form"] = "Semi-Detached"  #'End-Terrace'
    df.loc[df.House_Form == "Semi-Detatched", "House_Form"] = "Semi-Detached"
    df.loc[df.House_Form == "Flat", "House_Form"] = "Flat/Apartment"
    df.loc[df.House_Form == "End-Terrace", "House_Form"] = "Semi-Detached"  #'End-Terrace'

    # NB. The rule that we follow here is to map house age to the lowest age band. As pointed by
    # SY this can lead to cross-contamination of the bands e.g. a house with initial band '1945-1964'
    # will endup in band '1930-1949' where in fact a bigger fraction of that band belongs to the
    # '1950-1064' band. This means it should be followed by multiple training test rounds to assess the
    # the impact of that choice.
    df.House_Age = df.House_Age.str.replace("to", "-")
    df.House_Age = df.House_Age.str.replace(" onwards", "+")
    df.House_Age = df.House_Age.str.replace(" ONWARDS", "+")
    df.House_Age = df.House_Age.str.replace(" ", "")
    df.loc[(df.House_Age == "Before1900") | (df.House_Age == "before1900"), "House_Age"] = "Before 1900"
    df.loc[df.House_Age == "2001+", "House_Age"] = "1996-2002"
    df.loc[
        (df.House_Age == "Pre-1919")
        | (df.House_Age == "Pre1919")
        | (df.House_Age == "1901-1929")
        | (df.House_Age == "1919-1944"),
        "House_Age",
    ] = "1900-1929"
    df.loc[df.House_Age == "1945-1964", "House_Age"] = "1930-1949"
    df.loc[df.House_Age == "1965-1980", "House_Age"] = "1967-1975"
    df.loc[df.House_Age == "1967-1976", "House_Age"] = "1967-1975"
    df.loc[df.House_Age == "1981-1990", "House_Age"] = "1983-1990"
    df.loc[df.House_Age == "1991-2000", "House_Age"] = "1991-1995"
    h_ind = df.House_Age.str.contains("-")
    df.loc[h_ind, "House_Age"] = df.loc[h_ind]["House_Age"].str[0:4] + " - " + df.loc[h_ind]["House_Age"].str[5:]

    df.loc[df.House_Env == "Surburban", "House_Env"] = "Suburban"
    df.loc[df.House_Env == "surburban", "House_Env"] = "Suburban"
    df.loc[df.House_Env == "Unknown", "House_Env"] = "Suburban"

    df.loc[df.Glazed_Type == "double", "Glazed_Type"] = "Double"
    df.loc[df.Glazed_Type == "single", "Glazed_Type"] = "Single"

    df.loc[df.Floor_Type == "suspended", "Floor_Type"] = "Suspended"
    df.loc[df.Floor_Type == "solid", "Floor_Type"] = "Solid"
    df.loc[df.Floor_Type == "mix", "Floor_Type"] = "Mix"

    df.loc[df.TS_Existing_Size == "Normal (90-130l)", "TS_Existing_Size"] = "110"
    df.loc[df.TS_Existing_Size == "Large (> 170l)", "TS_Existing_Size"] = "200"
    df.loc[df.TS_Existing_Size == "Medium (131-170l)", "TS_Existing_Size"] = "150"
    df.loc[df.TS_Existing_Size == "unknown", "TS_Existing_Size"] = "110"
    df.TS_Existing_Size.fillna(0, inplace=True)
    df.loc[df.TS_Existing_Size == "", "TS_Existing_Size"] = 0
    df.loc[df.TS_Existing_Size == "N/A", "TS_Existing_Size"] = 0
    df.loc[df.TS_Existing_Size == "n/a", "TS_Existing_Size"] = 0
    df.TS_Existing_Size = df.TS_Existing_Size.astype(int)

    df.loc[df.Wall_Type == "cavity_insulated", "Wall_Type"] = "Cavity - filled"
    df.loc[df.Wall_Type == "Cavity_No_insulation", "Wall_Type"] = "Cavity - unfilled"
    df.loc[df.Wall_Type == "cavity_no_insulation", "Wall_Type"] = "Cavity - unfilled"
    df.loc[df.Wall_Type == "solid_no_insulation", "Wall_Type"] = "Solid - uninsulated"
    df.loc[df.Wall_Type == "Solid_insulated", "Wall_Type"] = "Solid - Insulated"
    df.loc[df.Wall_Type == "solid_no_ insulation", "Wall_Type"] = "Solid - uninsulated"
    df.loc[df.Wall_Type == "Solid_No_insulation", "Wall_Type"] = "Solid - uninsulated"
    df.loc[df.Wall_Type == "Cavity_Insulated", "Wall_Type"] = "Cavity - filled"
    df.loc[df.Wall_Type == "Cavity_No_Insulation", "Wall_Type"] = "Cavity - unfilled"
    df.loc[df.Wall_Type == "Solid_Insulated", "Wall_Type"] = "Solid - Insulated"
    df.loc[df.Wall_Type == "Solid_No_Insulation", "Wall_Type"] = "Solid - uninsulated"

    df["underfloor"] = "0"
    df.loc[df.No_Underfloor == "", "No_Underfloor"] = np.nan
    df.loc[df.No_Underfloor == "nan"] = np.nan
    df.loc[df.No_Underfloor == "o"] = np.nan
    df.No_Underfloor = df.No_Underfloor.astype(float)
    df.loc[df.No_Underfloor > 0, "underfloor"] = "1"

    df.loc[df.Total_Floor_Area == "", "Total_Floor_Area"] = np.nan
    df.Total_Floor_Area = df.Total_Floor_Area.astype(float)
    df["House_Size"] = "<50m2"
    df.loc[(df.Total_Floor_Area >= 50) & (df.Total_Floor_Area < 70), "House_Size"] = "50-70m2"
    df.loc[(df.Total_Floor_Area >= 70) & (df.Total_Floor_Area < 90), "House_Size"] = "70-90m2"
    df.loc[(df.Total_Floor_Area >= 90) & (df.Total_Floor_Area < 110), "House_Size"] = "90-110m2"
    df.loc[(df.Total_Floor_Area >= 110) & (df.Total_Floor_Area < 200), "House_Size"] = "110-200m2"
    df.loc[(df.Total_Floor_Area >= 200) & (df.Total_Floor_Area < 300), "House_Size"] = "200-300m2"
    df.loc[(df.Total_Floor_Area >= 300) & (df.Total_Floor_Area < 400), "House_Size"] = "300-400m2"
    df.loc[df.Total_Floor_Area > 400, "House_Size"] = "300-400m2"

    df.loc[(df.Bedrooms == "") | (df.Bedrooms.isna()), "Bedrooms"] = 0
    df.loc[df.Bedrooms == "5+", "Bedrooms"] = 5
    df.Bedrooms = df.Bedrooms.astype(int)
    df.loc[df.Bedrooms >= 5, "Bedrooms"] = "5+"
    df.Bedrooms = df.Bedrooms.astype(str)

    return df


def create_output_table() -> pd.DataFrame:
    """Function that creates the output table with the required inputs as rows

    Returns:
        pd.DataFrame: Table with the required rows
    """

    d = {}
    d["House_Form"] = ["Detached", "Semi-Detached", "Mid-Terrace", "Flat/Apartment", "Park Home"]
    d["Wall_Type"] = ["Solid - uninsulated", "Solid - Insulated", "Cavity - unfilled", "Cavity - filled"]
    d["House_Age"] = [
        "Before 1900",
        "1900 - 1929",
        "1930 - 1949",
        "1950 - 1966",
        "1967 - 1975",
        "1976 - 1982",
        "1983 - 1990",
        "1991 - 1995",
        "1996 - 2002",
        "2003 - 2006",
        "2007 - 2011",
        "2012+",
    ]
    d["House_Size"] = [
        "<50m2",
        "50-70m2",
        "70-90m2",
        "90-110m2",
        "110-200m2",
        "200-300m2",
        "300-400m2",
    ]
    d["Roof_Type"] = [
        "Loft - uninsulated",
        "Loft - partially insulated or unknown amount of insulation",
        "Loft - insulated",
        "A flat roof - uninsulated",
        "A flat roof - insulated",
        "Loft conversion - uninsulated",
        "Loft conversion - insulated",
        "No roof (ground or mid-floor flat)",
    ]
    d["Glazing"] = ["Single", "Double", "Triple"]
    d["Gas_Supply"] = ["Yes", "No"]
    d["Outside_Space"] = ["Yes", "No"]
    d["Current_System"] = [
        "Natural Gas",
        "Oil",
        "LPG",
        "Electricity",
        "Non-renewable solid fuel (e.g. coal)",
        "Biomass / Biogas",
    ]
    df1 = pd.DataFrame(d["House_Form"], columns=["House_Form"])
    df1["key"] = 0
    df2 = pd.DataFrame(d["Wall_Type"], columns=["Wall_Type"])
    df2["key"] = 0
    df_out = pd.merge(df1, df2, on="key")
    for k in ["House_Age", "House_Size", "Roof_Type", "Glazing", "Gas_Supply", "Outside_Space", "Current_System"]:
        df1 = pd.DataFrame(d[k], columns=[k])
        df1["key"] = 0
        df_out = pd.merge(df_out, df1, on="key")
    return df_out


def get_calc_outputs(df_out: pd.DataFrame, include_running_cost_comparison: bool) -> pd.DataFrame:
    """Fill the given dataframe with calculated efficiency and cost saving values

    Args:
        df_out (pd.DataFrame): Inout dataframe

    Returns:
        pd.DataFrame: Output dataframe
    """
    # Load cost and CO2 savings from excel file
    df_cost_savings = pd.read_excel(
        os.path.join(os.getcwd(), "Cost and Emissions Comparison.xlsx"),
        sheet_name="Comparison Matrix",
        header=1,
        usecols="C:M",
        nrows=10,
    )
    df_cost_savings.rename(columns={"Unnamed: 2": "Current System"}, inplace=True)
    df_CO2_savings = pd.read_excel(
        os.path.join(os.getcwd(), "Cost and Emissions Comparison.xlsx"),
        sheet_name="Comparison Matrix",
        header=14,
        usecols="C:M",
        nrows=10,
    )
    df_CO2_savings.rename(columns={"Unnamed: 2": "Current System"}, inplace=True)

    # Set suitability of heatpumps for certain houses
    df_out["lt_ashp_suitable"] = 1  # all houses gwt ASHP
    df_out["ht_ashp_suitable"] = 1  # all houses gwt HT ASHP
    df_out.loc[df_out["House_Form"] == "Park Home", "lt_ashp_suitable"] = 2  # parkhomes are unsuitable
    df_out.loc[df_out["House_Form"] == "Park Home", "ht_ashp_suitable"] = 2  # parkhomes are unsuitable

    df_out["gshp_suitable"] = 2
    df_out.loc[(df_out["Outside_Space"] == "Yes") & ~(df_out["House_Form"] == "Park Home"), "gshp_suitable"] = 1

    df_out["hhp_suitable"] = 2
    df_out.loc[(df_out["Gas_Supply"] == "Yes") & ~(df_out["House_Form"] == "Park Home"), "hhp_suitable"] = 1

    # Mapping the CO2 and cost savings to the output dataframe from data from excel file
    column_map = {
        "lt_ashp": "Air Source Heat Pump",
        "ht_ashp": "HT ASHP",
        "gshp": "Ground Source Heat Pump",
        "hhp": "Hybrid Heat Pump",
    }
    system_map = {
        "Natural Gas": "Natural Gas Boiler",
        "Oil": "Oil Fired Boiler",
        "LPG": "LPG Boiler",
        "Electricity": "Electrical Resistance Heaters",
        "Non-renewable solid fuel (e.g. coal)": "Non-Renewable Solid Fuel Boiler",
        "Biomass / Biogas": "Biomass Boiler",
    }
    for col_key, col_value in column_map.items():
        # By default savings are nan - If we have values for them they will get over written later
        emission_col_name = f"{col_key}_emission_savings"
        df_out[emission_col_name] = np.nan

        # By default savings are nan - If we have values for them they will get over written later
        # Currently we are just populating the min cost column here although this may change in the future (max is nan for now)
        cost_col_name = f"{col_key}_run_cost_min"
        df_out[cost_col_name] = np.nan
        df_out[f"{col_key}_run_cost_max"] = np.nan

        for row_key, row_value in system_map.items():
            CO2_saving = df_CO2_savings.loc[df_CO2_savings["Current System"] == row_value, col_value].values[0]
            cost_saving = df_cost_savings.loc[df_cost_savings["Current System"] == row_value, col_value].values[0]

            # Input the emission savings
            df_out.loc[df_out["Current_System"] == row_key, emission_col_name] = round(CO2_saving, 2)

            # Input the cost savings
            if include_running_cost_comparison:
                df_out.loc[df_out["Current_System"] == row_key, cost_col_name] = round(cost_saving, 2)

    return df_out

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from joblib import dump
import os

from sklearn import preprocessing
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.neural_network import MLPClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import ConfusionMatrixDisplay, classification_report


def create_hp_bins_column(df, hp_bins=[1, 7, 10.5], load_column="HP_Size_kW"):
    df["HP_Size_kW_bin"] = np.nan
    df.loc[df[load_column] < hp_bins[1], "HP_Size_kW_bin"] = 1
    df.loc[(df[load_column] >= hp_bins[1]) & (df[load_column] <= hp_bins[2]), "HP_Size_kW_bin"] = 2
    df.loc[df[load_column] > hp_bins[2], "HP_Size_kW_bin"] = 3
    return df


def get_model_input_df(
    df, df_out, hp_bins, cat_in_cols=["House_Age", "House_Size", "House_Form", "Wall_Type"], target="HP_Size_kW_bin"
):
    for cat_col in cat_in_cols:
        df = df.loc[df[cat_col] != "", :]

    if target == "HP_Size_kW_bin":
        df["HP_Size_kW_bin"] = np.nan
        df.loc[df["HP_Size_kW"] < hp_bins[1], "HP_Size_kW_bin"] = 1
        df.loc[(df["HP_Size_kW"] >= hp_bins[1]) & (df["HP_Size_kW"] <= hp_bins[2]), "HP_Size_kW_bin"] = 2
        df.loc[df["HP_Size_kW"] > hp_bins[2], "HP_Size_kW_bin"] = 3

    # drop nans here before the one hot encoding.
    df = df[cat_in_cols + [target, "total_cost", "HP_Installed"]].dropna()

    # Perform one-hot encoding (i.e. one column per value) for non-ordinal categorical columns
    le = {}
    d_model_in = {}
    d_train = {}
    for cat_col in cat_in_cols:
        le[cat_col] = preprocessing.LabelEncoder()
        le[cat_col].fit(df_out[cat_col].values)
        dump(le[cat_col], "model_artefacts/le_" + cat_col + ".joblib")
        d_model_in[cat_col] = le[cat_col].transform(df_out[cat_col].values)
        d_train[cat_col] = le[cat_col].transform(df[cat_col].values)

    df_in = pd.DataFrame.from_dict(d_train)
    df_in = pd.get_dummies(pd.DataFrame(d_train).astype(str))
    df_in[target] = df[target]  # this is to make sure every row gets a target variable

    # these last two columns are for evaluation
    df_in["total_cost"] = df["total_cost"]  # this is to make sure every row gets a target variable
    df_in["HP_Installed"] = df["HP_Installed"]

    return df_in.dropna().reset_index()  # resetting index so we can use it for the train test split


def out_of_bounds_metric(
    df_in,
    y_test,
    y_pred_hp_size_lr,
    y_pred_hp_size_rf,
    y_pred_hp_size_nn,
    id_test,
    hp_costs,
    hp_type="ASHP",
    verbose=True,
):
    """
    Function that calculates our main metric for model accuracy. The fraction of houses that were assigned a HP price that lies
    outside the price range the model assigned to those houses.

    """
    df_model_results = pd.DataFrame(
        {
            "y_test": y_test,
            "y_pred_hp_size_lr": y_pred_hp_size_lr,
            "y_pred_hp_size_rf": y_pred_hp_size_rf,
            "y_pred_hp_size_nn": y_pred_hp_size_nn,
            "y_test_cost": df_in.iloc[id_test]["total_cost"],
            "y_test_hp_model": df_in.iloc[id_test]["HP_Installed"],
        }
    )
    df_model_results = df_model_results.loc[df_model_results["y_test_hp_model"].isin([hp_type]), :]

    df_model_results["lr_cost_min"] = np.nan
    df_model_results["lr_cost_max"] = np.nan
    out_of_bounds = {}
    in_bounds = {}
    if verbose:
        print("For ", hp_type)
    for m in ["lr", "rf", "nn"]:
        if verbose:
            print("Working with model:", m)
        for i in range(1, 4):
            df_model_results.loc[df_model_results["y_pred_hp_size_" + m] == i, m + "_cost_min"] = hp_costs[i][0]
            df_model_results.loc[df_model_results["y_pred_hp_size_" + m] == i, m + "_cost_max"] = hp_costs[i][1]

        df_outside_bounds = df_model_results.loc[
            (df_model_results["y_test_cost"] < df_model_results[m + "_cost_min"])
            | (df_model_results["y_test_cost"] > df_model_results[m + "_cost_max"]),
            ["y_test", "y_pred_hp_size_" + m, "y_test_cost", m + "_cost_min", m + "_cost_max"],
        ]

        df_inside_bounds = df_model_results.loc[
            (df_model_results["y_test_cost"] >= df_model_results[m + "_cost_min"])
            & (df_model_results["y_test_cost"] <= df_model_results[m + "_cost_max"]),
            ["y_test", "y_pred_hp_size_" + m, "y_test_cost", m + "_cost_min", m + "_cost_max"],
        ]

        # if negative then is out of bounds either under or over
        df_outside_bounds["cost_discr_under"] = df_outside_bounds["y_test_cost"] - df_outside_bounds[m + "_cost_min"]
        df_outside_bounds["cost_discr_over"] = df_outside_bounds[m + "_cost_max"] - df_outside_bounds["y_test_cost"]
        df_outside_bounds["cost_discr_over_pc"] = (
            df_outside_bounds["cost_discr_over"] / df_outside_bounds["y_test_cost"]
        )
        df_outside_bounds["cost_discr_under_pc"] = (
            df_outside_bounds["cost_discr_under"] / df_outside_bounds["y_test_cost"]
        )
        df_outside_bounds["out_of_bounds_flg"] = 1

        df_inside_bounds["cost_discr_under"] = 0
        df_inside_bounds["cost_discr_over"] = 0
        df_inside_bounds["cost_discr_over_pc"] = 0
        df_inside_bounds["cost_discr_under_pc"] = 0
        df_inside_bounds["out_of_bounds_flg"] = 0

        out_of_bounds[m] = pd.concat([df_outside_bounds, df_inside_bounds])
        if verbose:
            print("out of bounds:", np.int32(100 * len(df_outside_bounds) / len(df_model_results)), "%")
    return out_of_bounds


def consolidate_hp_types(df_hp):
    print(df_hp["HP_Installed"].value_counts())
    df_hp.loc[df_hp["HP_Installed"].str.startswith("Hybrid"), "HP_Installed"] = "Hybrid"
    df_hp.loc[df_hp["HP_Installed"].str.startswith("GSHP"), "HP_Installed"] = "GSHP"
    return df_hp


def run_model(df_hp_model, df_out, hp_bins, hp_costs, save_artefacts=False, do_plots=False):
    if not os.path.isdir("model_artefacts"):
        os.mkdir("model_artefacts")
    df_in = get_model_input_df(df_hp_model, df_out, hp_bins)
    print(df_in.columns.values)
    X = df_in[df_in.columns[1:-3]].values  # by construction of df second to last column is the target
    y = df_in["HP_Size_kW_bin"]
    ids = df_in.index.values
    X_train, X_test, y_train, y_test, id_train, id_test = train_test_split(X, y, ids, test_size=0.4, random_state=42)

    # Even though the input values are between 0 and 1 we scale so that the inputs are centered around 0.
    scaler = preprocessing.StandardScaler().fit(X_train)

    if save_artefacts:
        dump(scaler, "model_artefacts/scaler.joblib")

    X_train = scaler.transform(X_train)

    lr = LogisticRegression().fit(X_train, y_train)

    print("working with random forest estimator...")
    rf_param_grid = {
        "min_samples_leaf": [3, 4, 5],
        "min_samples_split": [8, 10, 12],
        "n_estimators": [100, 200, 300, 1000],
    }
    rf_clf = RandomForestClassifier()
    rf = GridSearchCV(estimator=rf_clf, param_grid=rf_param_grid, cv=9, n_jobs=-1, verbose=2)
    rf.fit(X_train, y_train)

    print("working with nn estimator...")
    nn_param_grid = {
        "hidden_layer_sizes": [(50, 50, 50), (50, 100, 50), (100,)],
        "activation": ["tanh", "relu"],
        "solver": ["sgd", "adam"],
        "alpha": [0.0001, 0.05],
        "learning_rate": ["constant", "adaptive"],
    }
    nn_clf = MLPClassifier(max_iter=1000)
    nn = GridSearchCV(estimator=nn_clf, param_grid=nn_param_grid, cv=9, n_jobs=-1, verbose=2)
    nn.fit(X_train, y_train)
    X_test = scaler.transform(X_test)
    y_pred_hp_size_lr = lr.predict(X_test)
    y_pred_hp_size_rf = rf.predict(X_test)
    y_pred_hp_size_nn = nn.predict(X_test)

    if save_artefacts:
        dump(rf, "model_artefacts/eoh_rf.joblib")
        dump(lr, "model_artefacts/eoh_lr.joblib")
        dump(nn, "model_artefacts/eoh_nn.joblib")

    if do_plots:
        ConfusionMatrixDisplay.from_predictions(y_test, y_pred_hp_size_lr)
        plt.title("Logistic regression")
        ConfusionMatrixDisplay.from_predictions(y_test, y_pred_hp_size_rf)
        plt.title("Random Forest")
        ConfusionMatrixDisplay.from_predictions(y_test, y_pred_hp_size_nn)
        plt.title("Neural Network")

        print("Logistic Regression")
        print(classification_report(y_test, y_pred_hp_size_lr))
        print("Random Forest")
        print(classification_report(y_test, y_pred_hp_size_rf))
        print("Neural Networks")
        print(classification_report(y_test, y_pred_hp_size_nn))

    out_of_bounds = out_of_bounds_metric(
        df_in, y_test, y_pred_hp_size_lr, y_pred_hp_size_rf, y_pred_hp_size_nn, id_test, hp_costs, hp_type="ASHP"
    )
    out_of_bounds_input = (df_in, y_test, y_pred_hp_size_lr, y_pred_hp_size_rf, y_pred_hp_size_nn, id_test, hp_costs)
    return out_of_bounds, out_of_bounds_input

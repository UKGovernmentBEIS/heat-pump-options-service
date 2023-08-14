# Heat Pumps Options Service - Analysis  

This analysis relies on an unpublished dataset. As a result, the code provided here can only be run by those with access to this unpublished data. Once this data is made open, the code here will be updated to run using the open dataset.  

This file documents the process for running the code used in the analysis part of the heat pumps options service project.  


---

## Files  
hp_models5.ipynb - Python notebook builds and evaluates models of heat pump installation costs.  
eoh_table_build.ipynb - Python notebook builds the database for the heat pumps options service.  
eoh_table_aux.py - Auxiliary python functions for use in the notebooks.  
model_functions.py - Auxiliary python functions for use in the hp_models5 notebook.  
Cost and Emissions Comparison.xlsx - Cost and emissions comparison tables used in eoh_table_build notebook to build the database.  
urls_v2.json - File containing urls to access datasets through USMART.io.  
anaconda_environment.yml - Anaconda environment config.  
model_artefacts/selected_model.joblib files - Model file for the best performing model selected for use  
ashp_costs_table.csv - Output air-source hp cost data from hp_models5.ipynb used in eoh_table_build.ipynb  
ht_ashp_costs_table.csv - Output high-temperature air-source hp cost data from hp_models5.ipynb used in eoh_table_build.ipynb  
gshp_costs_table.csv - ground source hp costs from manually specified and used in eoh_table_build.ipynb  


## Abbreviations
hp - heat pump
ashp - air-source heat pump
ht_ashp - high temperature air-source heat pump
gshp - ground-source heat pump
hhp - hybrid heat pump

## Running order
To perform the analysis and reproduce the database you must run the notebooks in the following order:
hp_models5.ipynb  
eoh_table_build.ipynb  


## Configuration
To run the notebooks there is some configuration that will be required.  
- Clone the anaconda environment from the config file   

    conda env create -f anaconda_environment.yml   

This will create an anaconda environment called "HPOS_analysis" with all the required python packages. This environment can then be used when running the notebooks.  

- Add environment variables to access USMART.io  

    USMART_KEY_ID
    USMART_KEY_SECRET

This will require a USMART account. Once you log in you can find your API key ID and secret by navigating to profile in the top right.

## Public Dataset Change
The analysis code is written to read the private datasets as it was written prior to dataset publication. The non-restricted (GDPR) data has now been cleansed and collected into a single published dataset. As the data is published on a government open source license, the code can be rewritten to access and read from this dataset without requiring the permissions which are needed to read the private data. 

This work will require changes to the definition of the get_survey_datasets() function in the “eoh_table_aux.py” file. 

URLs are read in from urls_v2.json as the private data stored at multiple URLs – The public version is at a single URL. (API: https://api.usmart.io/org/92610836-6c2b-4a26-a0a0-b903bde0dc46/231dd812-ed7f-41b4-bbe2-c0929ca95299/latest/urql ). Code changes will be required to account for this. 

There may also be changes in the column names between the private and public datasets, corresponding changes to these fields will need to made in the code.

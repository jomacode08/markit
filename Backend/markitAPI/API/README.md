# Markit API

## Run for develop 
1. Copy the file `appsettings-template.json` and generate the file `appsettings.json`, ensure that you fill the required information for the develop env
	- Fill the information of the connection string for the develop database
	- Fill the default credentials for the Admin -> if it's the first time that you are going to create the database. Otherwise, updating this information will not make any changes.

2. Ensure if you have any migration pending to create
3. Start IIS Express server, if you create a new migration, it would apply automatically once the app start.
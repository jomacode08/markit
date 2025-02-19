# Markit API

## Run for develop 
1. Copy the file `appsettings-template.json` and generate the file `appsettings.json`, ensure that you fill the required information for the develop env
	- Fill the `ConnectionStrings.ConnectionString` property with the develop database connection
	- Fill the `GoogleSettings.ClientId` that the app uses for authentication purposes
	- Fill the `UserDefaultSettings` with the default credentials for the Admin -> if it's the first time that you are going to create the database. Otherwise, updating this information will not make any changes.

2. Ensure if you have any migration pending to create

3. Start the app. If you create a new migration, it would apply automatically once the app start.
	- Start the IIS Express server
	- Or you can run the app using dotnet CLI, see `launchSettings.json` file to review the launch profiles.
		- Run the following commands on the root folder of the API project `markit/Backend/markitAPI/API`
		- `dotnet clean`
		- `dotnet watch run --environment Development --launch-profile "Development"`
		- The app should be running on `http://localhost:5000`, to see the Swagger UI go to: `http://localhost:5000/swagger`
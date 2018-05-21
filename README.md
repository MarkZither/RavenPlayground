# RavenPlayground

## How to run this demo

* [Download the server](https://ravendb.net/download)
* Unzip the downloaded archive.
* open a powershell console and run `unzippedDirectory\run.ps1`. This will start the setup wizard, 
make sure this is run in Chrome (or Edge?) or you will have to do additional steps with certificates.
* Follow the Let's Encrypt instructions in the [Installation : Setup Wizard Walkthrough](https://ravendb.net/docs/article-page/4.0/csharp/start/installation/setup-wizard)
> If you checked the relevant box in the previous stage, a client certificate is registered in the OS trusted store during setup. 
> The Chrome and Edge browsers use the OS store, so they will let you choose your certificate right before you are redirected. Firefox users will have to manually import the certificate to the browser via Tools > Options > Advanced > Certificates > View Certificates.

If the certifcates have registered properly you should now be able to restart the server and access the dashboard.

* Create a new database
* Add enviroment variables for the following values
 - `Environment.GetEnvironmentVariable("certPassword")`
 - `Environment.GetEnvironmentVariable("certLocation")` where you extracted the client cert to in the setup or better still where you put the new client cert.
 - `Environment.GetEnvironmentVariable("ravenDBServer")` make sure you use the domain name to match the certificate or you will get TLS errors.
 - `Environment.GetEnvironmentVariable("stackAppsKey")` [Register an app](https://stackapps.com/apps/oauth/register)
 
 Run the console app, it will lead you through the rest.

 ## Project Gutenberg Data
 to download all English books in txt format run `wget -w 2 -m -H "http://www.gutenberg.org/robot/harvest?filetypes[]=txt&langs[]=en"` and [download the index file](https://www.gutenberg.org/cache/epub/feeds/rdf-files.tar.zip) and unzip and extract the tar to a directory `rdf-files` in the root of the gutenberg directory used in the console app.

# Heat Pumps Option Service

## About the service

Find out heat pump installation options for your home.

Use this service to find out:

* more information about heat pumps,
* which heat pumps may be options for installation in your home,
* the approximate cost of a heat pump installation in your home,
* how a heat pump installation might affect your carbon emissions and ongoing heating bill.


## Project Structure
The project is built around .NET Core 3.1 ASP.NET Core Web App using Razor pages.  


## EPC API
The [Domestic Energy Performance Certificates API][1] requires a registered
account to be obtained in order to use it, registration can be obtained from
the [registration][2] page and requires acceptance of the license and
copyright---details available on the [copyright page][3]

## Postcodes
To identify the country from postcodes the Uses api.getthedata.com which is
licensed under an [Open Government License][4] which, due to licensing
restrictions does not support addresses in Northern Ireland.


## Build and test

The project is built around .NET Core 3.1 ASP.NET Core Web App using Razor pages. You will need both a .NET Core 3.1 SDK and a program capable of building C#. Visual Studio 2019 provides both of these.

* To build the project with Visual Studio 2019, open hsm/HSM.sln and run Build > Build Solution.
* To run the project, first register to the Domestic Energy Performance Certificates API from the [registration page][2] and obtain a key. Add the email address with which you created and account and the value of the obtained key to the secrets.json file under a section named "AppConfigSettings" - AppConfigSettings.EPCServiceEmail and AppConfigSettings.EPCServiceKey respectively.
* To run the included tests, run Test > Run All Tests.

[1]: https://epc.opendatacommunities.org/docs/api/domestic (EPC API)
[2]: https://epc.opendatacommunities.org/#register (EPC registration)
[3]: https://epc.opendatacommunities.org/docs/copyright (Copyright)
[4]: http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/


## Dependencies

| Package                   |  Version       |  License          |
|---------------------------|:---------------|:-----------------:|
| ElmahCore                 |  1.2.5         |  Apache 2.0       | 
| ElmahCore.Sql             |  1.2.5         |  Apache 2.0       |
| Newtonsoft.Json           |  12.0.3        |  MIT              |
| Serilog                   |  2.9.0         |  Apache 2.0       |
| Serilog.Sinks.RollingFile |  3.3.0         |  Apache 2.0       |
| Serilog.Sinks.Map         |  1.0.1         |  Apache 2.0       |
| xunit                     |  2.4.1         |  Apache 2.0       |
| xunit.runner.visualstudio |  2.4.2         |  Apache 2.0       |





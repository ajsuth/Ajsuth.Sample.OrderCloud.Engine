# Sitecore Experience Commerce to OrderCloud Migration
This is sample plugin to migrate Sitecore Commerce data over into OrderCloud.

## Supported Sitecore Experience Commerce Versions
- XC 10.1

## Features

* Customer (Buyer) Migration
* Catalog Migration
* Category Migration
* Sellable Item (Product) Migration
* Price Migration
* Inventory Migration

## Installation Instructions
1. Download the repository.
2. Add the **Ajsuth.Sample.OrderCloud.Engine.csproj** to the _**Sitecore Commerce Engine**_ solution.
3. In the _**Sitecore Commerce Engine**_ project, add a reference to the **Ajsuth.Sample.OrderCloud.Engine** project.
4. Copy **PlugIn.OrderCloud.PolicySet-1.0.0.json** from _\data\Environments_ into the same location in the **Sitecore Commerce Engine** project.
5. Update the **OrderCloudClientPolicy** properties in the **OrderCloudPolicySet** with your OrderCloud credentials.
6. Add a reference to **OrderCloudPolicySet** to the appropriate commerce role configuration, e.g. **PlugIn.Habitat.CommerceAuthoring-1.0.0.json**.
7. Run the _**Sitecore Commerce Engine**_ from Visual Studio or deploy the solution and run from IIS.
8. Run the Bootstrap command on the _**Sitecore Commerce Engine**_.

## Known Issues
| Feature                 | Description | Issue |
| ----------------------- | ----------- | ----- |
|                         |             |       |

## Disclaimer
The code provided in this repository is sample code only. It is not intended for production usage and not endorsed by Sitecore.
Both Sitecore and the code author do not take responsibility for any issues caused as a result of using this code.
No guarantee or warranty is provided and code must be used at own risk.

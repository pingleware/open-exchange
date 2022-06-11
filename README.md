# Open Exchange
Cloned from https://sourceforge.net/projects/open-exchange/

OpEx is an application suite that includes the main building blocks of commercial electronic trading systems.

All OpEx applications run on distributed system architectures.

# Features

  - Trading Screen: order entry, amendment, cancellation; order and trade blotters; trade and visualise market data for multiple securities
  - Order Manager: receives order instructions from order generators, sends order instructions to order destinations; routes orders to the appropriate destination based on configurable routing rules; persists order and execution data to the DB
  - Matching Exchange: executes order instructions; implements market and limit orders; configurable per-instrument preferences: minqty, maxqty, minprice, maxprice, pricetick; acts as a source of real time market data
  - Algorithmic Trading Agents: listen to market data to make trading decisions; per-agent configurable behaviour

![](https://a.fsdn.com/con/app/proj/open-exchange/screenshots/TTGUI.png/max/max/1)
![](https://a.fsdn.com/con/app/proj/open-exchange/screenshots/AdminGUI.png/max/max/1)
![](https://a.fsdn.com/con/app/proj/open-exchange/screenshots/OrderGenerator.png/max/max/1)

# TODO
The purpose of the build is to make an open source ATS that includes EVM Smart Contracts which can be registered with the SEC.

  - Add ATS reporting to the Admin GUI
  - Add Ethereum Virtual Machine for executing contracts (see https://www.nuget.org/packages/Nethereum)

# OpEx Reference
## Standard Components
### Configuration Server
The Configuration Server (CS) caches application configuration data it periodically retrieves from the DB, and passes the data to the applications.
The CS identifies an instance of OpEx: all OpEx applications of one instance will connect to the CS of that particular instance at startup, and retrieve their configuration from there.

#### Configuration
A sample configuration file is provided below.

  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
    <appSettings>
      <add key="LogFolder" value="C:\OPEX\Log"/>

      <add key="CSChannelName" value="ConfigurationServiceChannel"/>
      <add key="CSPort" value="12000"/>
      <add key="CSUri" value="ConfigurationService.rem"/>

      <add key="DBSchemaName" value="opex"/>
      <add key="DBUserName" value="opex"/>
      <add key="DBPort" value="3306"/>
      <add key="DBPassword" value="password"/>
      <add key="DBHostName" value="localhost"/>

      <add key="PollInterval" value="10"/>
    </appSettings>
  </configuration>

Variable|Default|Meaning
:----- | :----: | -----:
LogFolder|	.|	The log folder of the application
CSChannelName|	N/A|	The name of the channel the application will use to publish configuration
CSPort|	N/A|	The port the application will use to publish configuration
CSUri|	N/A|	The Uri the application will use to publish configuration
DBSchemaName|	N/A|	The name of the OpEx DB schema
DBUserName|	N/A|	The user name of the OpEx DB
DBPort|	N/A|	The port of the OpEx DB
DBPassword|	N/A|	The password of the OpEx DB
DBHostName|	N/A|	The name of the host where the OpEx DB is running
PollInterval|	N/A|	The time interval, in seconds, with which the application will poll the OpEx DB to reload the configuration
#### Notes
The first time you run CS, a Windows Security Alert may popup, warning you that CS tried to connect to a destination on your network and it was blocked by Windows Firewall. You need to click on "Allow access" to let CS connect to the DB, and listen to incoming connection requests from OpEx applications.
![](https://sourceforge.net/p/open-exchange/wiki/Reference%20-%20Configuration%20Server/attachment/CSFirewall.png)
### Order Manager
The order manager routes the orders it receives from order generators (such as GUIs or Agents) to the appropriate order destination (Matching Exchange).

#### Configuration
A sample configuration file is provided below.

  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
    <appSettings>
      <add key="LogFolder" value="C:\OPEX\Log"/>
      <add key="ApplicationName" value="OrderManager"/>

      <add key="CSChannelName" value="ConfigurationServiceChannel"/>
      <add key="CSHost" value="localhost"/>
      <add key="CSPort" value="12000"/>
      <add key="CSUri" value="ConfigurationService.rem"/>
    </appSettings>
  </configuration>

Variable|Default|Meaning
:----- | :----: | -----:
LogFolder|.|The log folder of the application
ApplicationName|N/A|The name of the OpEx application
CSChannelName|N/A|The name of the channel the application will use to retrieve its configuration
CSPort|N/A|The port the application will use to retrieve its configuration
CSUri|N/A|The Uri the application will use to retrieve its configuration
CSHost|N/A|The name of the host where the CS is running

It is recommendable that the additional configuration needed by Order Manager be in the Configuration table of the Database.

ConfigKey|Default|Meaning
:----- | :----: | -----:
OMAllowedDestinations|N/A|The comma separated list of the destinations where Order Manager will route orders (a destination is identified by its ApplicationName)
OMAllowedDestinationsHosts|N/A|The comma separated list of the hosts where the destinations specified in OMAllowedDestinations are running
OMMode|Client|Must be set to Server
PurgeQueuesOnStartup|true|Removes any non-processed Incoming Order messages in the queue at startup

### Matching Exchange
The Matching Exchange processes orders as it receives them from the Order Manager, implementing the Continuous Double Auction as a market mechanism.

#### Configuration
A sample configuration file is provided below.

  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <appSettings>
      <add key="LogFolder" value="C:\OPEX\Log" />
      <add key="ApplicationName" value="OPEX" />

      <add key="CSChannelName" value="ConfigurationServiceChannel" />
      <add key="CSHost" value="localhost" />
      <add key="CSPort" value="12000" />
      <add key="CSUri" value="ConfigurationService.rem" />    
    </appSettings>  
  </configuration>

Variable|Default|Meaning
:----- | :----: | -----:
LogFolder|.|The log folder of the application
ApplicationName|N/A|The name of the OpEx application
CSChannelName|N/A|The name of the channel the application will use to retrieve its configuration
CSPort|N/A|The port the application will use to retrieve its configuration
CSUri|N/A|The Uri the application will use to retrieve its configuration
CSHost|N/A|The name of the host where the CS is running

It is recommendable that the additional configuration below needed by Matching Exchange be in the Configuration table of the Database.


ConfigKey|Default|Meaning
:----- | :----: | -----:
AllowAmendments|true|Accept order instructions to amend orders
AllowCancellations|true|Accept order instructions to cancel orders
ClosePeriodDuration|00:00:10|The amount of time during which the exchange remains closed (close and open periods alternate cyclically)
MDSDataSource|OPEX|The symbolic name to use as market data source
NYSESpreadImprovement|false|Forces the NYSE spread improvement rule, for which only more aggressive orders than those currently in the orderbook are accepted
OpenPeriodDuration|00:02:50|The amount of time during which the exchange remains open (close and open periods alternate cyclically)
PurgeQueuesOnStartup|true|Removes any non-processed Incoming Order messages in the queue at startup

#### The Instrument Table
The Matching Exchange needs additional configuration to specify which financial products will be traded on it. Such configuration is found in the Instrument table of the OpEx DB, structured as shown in the table below.

Field Name|Sample|Description
:----- | :----: | -----:
RIC|VOD.L|The RIC (Reuters Instrument Code) of the product
ExchangeName|OPEX|The name of the exchange on which the product is traded
MinQty|1|The minimum number of contract tradable in one transaction
MaxQty|100000|The maximum number of contract tradable in one transaction
MinPrice|1|The minimum price at which the product can be traded
MaxPrice|1000|The maximum price at which the product can be traded
PriceTick|1|The smallest price change

### Trading GUI
The Trading GUI is a graphical application that lets you interact with the market.
You can enter new orders, change or cancel your previously entered orders, watch their execution and status in the blotters.

Configuration
A sample configuration file is provided below.

  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
    <appSettings>
      <add key="LogFolder" value="C:\OPEX\Log"/>
      <add key="ApplicationName" value="GUIdemo"/>

      <add key="DefaultRIC" value="VOD.L"/>

      <add key="CSChannelName" value="ConfigurationServiceChannel"/>
      <add key="CSHost" value="localhost"/>
      <add key="CSPort" value="12000"/>
      <add key="CSUri" value="ConfigurationService.rem"/>
    </appSettings>
  </configuration>

Variable|Default|Meaning
:----- | :----: | -----:
LogFolder|.|The log folder of the application
ApplicationName|N/A|The name of the OpEx application
DefaultRIC|N/A|The default product the application will use to fill in new order tickets
CSChannelName|N/A|The name of the channel the application will use to retrieve its configuration
CSPort|N/A|The port the application will use to retrieve its configuration
CSUri|N/A|The Uri the application will use to retrieve its configuration
CSHost|N/A|The name of the host where the CS is running

It is recommendable that the additional configuration needed by Trading GUI be in the Configuration table of the Database.

ConfigKey|Default|Meaning
:----- | :----: | -----:
IDGeneratorType|Sequential|Must be set to Random
PurgeQueuesOnStartup|true|Removes any non-processed Incoming Order messages in the queue at startup

## Simulation Tools
  - Agent Host
  - Discrete Event Simulator
  - Sales Trading GUI
  - Admin Console
## Algorithmic Trading Agents
  - ZIC
  - Sniper
  - ZIP
  - GD
  - GDX
  - AA

# SEC Alternative Trading Systems (ATS)
See https://www.sec.gov/foia/docs/atslist.htm

## Forms

Name|Description|URL
:----- | :----: | -----:
ATS|Initial operation report, amendment to initial operation report and cessation of operations report for alternative trading systems (PDF)|https://www.sec.gov/files/formats.pdf
ATS-N|NMS Stock Alternative Trading Systems (PDF)|https://www.sec.gov/files/formats-n.pdf
ATS-R|Quarterly report of alternative trading systems activities (PDF)|https://www.sec.gov/files/formats-r.pdf
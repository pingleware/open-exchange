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
Left   | Center | Left
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
### Matching Exchange
### Trading GUI
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

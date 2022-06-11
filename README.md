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

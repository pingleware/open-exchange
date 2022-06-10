-- MySQL dump 10.13  Distrib 5.5.20, for Win32 (x86)
--
-- Host: noise    Database: opex
-- ------------------------------------------------------
-- Server version	5.1.41-3ubuntu12.10

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Agents`
--

DROP TABLE IF EXISTS `Agents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Agents` (
  `AgentName` varchar(128) NOT NULL,
  `AgentType` varchar(45) NOT NULL,
  `WakeOnTimer` tinyint(1) NOT NULL,
  `WakeOnTrades` tinyint(1) NOT NULL,
  `WakeOnOrders` tinyint(1) NOT NULL,
  `SleepTimeMsec` int(10) unsigned NOT NULL,
  `Parameters` varchar(1024) NOT NULL,
  `InactivityTimerSleepTimeMsec` int(10) unsigned NOT NULL DEFAULT '0',
  `Active` varchar(45) NOT NULL DEFAULT 'true',
  `Owner` varchar(64) NOT NULL,
  PRIMARY KEY (`AgentName`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Configuration`
--

DROP TABLE IF EXISTS `Configuration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Configuration` (
  `ApplicationName` varchar(128) NOT NULL,
  `ConfigKey` varchar(128) NOT NULL,
  `ConfigValue` varchar(256) NOT NULL,
  PRIMARY KEY (`ApplicationName`,`ConfigKey`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DESFills`
--

DROP TABLE IF EXISTS `DESFills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DESFills` (
  `SimID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `Round` int(10) unsigned NOT NULL,
  `Move` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` double NOT NULL,
  `LimitPrice` double NOT NULL,
  `User` varchar(32) NOT NULL,
  `Counterparty` varchar(32) NOT NULL,
  `Instrument` varchar(32) NOT NULL,
  `Side` varchar(4) NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`SimID`,`Round`,`Move`,`DateSig`,`User`,`Counterparty`)
) ENGINE=InnoDB AUTO_INCREMENT=81033663317 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DESJobs`
--

DROP TABLE IF EXISTS `DESJobs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DESJobs` (
  `JID` int(11) NOT NULL,
  `Repeat` int(11) NOT NULL DEFAULT '1',
  PRIMARY KEY (`JID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DESShouts`
--

DROP TABLE IF EXISTS `DESShouts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DESShouts` (
  `SimID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `Round` int(10) unsigned NOT NULL,
  `Move` int(10) unsigned NOT NULL,
  `Side` varchar(5) NOT NULL,
  `Accepted` tinyint(1) NOT NULL,
  `Price` double NOT NULL,
  `User` varchar(20) NOT NULL,
  `Instrument` varchar(20) NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`SimID`,`Round`,`Move`)
) ENGINE=InnoDB AUTO_INCREMENT=81033663317 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DESSimulationSummary`
--

DROP TABLE IF EXISTS `DESSimulationSummary`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DESSimulationSummary` (
  `SimID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `Round` bigint(20) unsigned NOT NULL,
  `UserName` varchar(45) NOT NULL,
  `MaxThSplus` double NOT NULL,
  `EqPrice` double NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  `RoundStart` varchar(10) NOT NULL,
  PRIMARY KEY (`SimID`,`Round`,`UserName`,`DateSig`)
) ENGINE=InnoDB AUTO_INCREMENT=80606165226 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DWESimulation`
--

DROP TABLE IF EXISTS `DWESimulation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DWESimulation` (
  `SIMID` bigint(20) NOT NULL,
  `TimeSig` varchar(13) NOT NULL,
  `UserName` varchar(45) NOT NULL,
  `Side` varchar(4) DEFAULT NULL,
  `Price` double DEFAULT NULL,
  `Quantity` int(11) DEFAULT NULL,
  `RIC` varchar(45) DEFAULT NULL,
  `CCY` varchar(3) DEFAULT NULL,
  `WaitTime` double DEFAULT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`SIMID`,`DateSig`,`TimeSig`,`UserName`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

--
-- Table structure for table `DWEPermits`
--

DROP TABLE IF EXISTS `DWEPermits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DWEPermits` (
  `SID` int(11) NOT NULL,
  `UserName` varchar(128) NOT NULL,
  `PermitType` int(11) NOT NULL,
  `Price` double DEFAULT NULL,
  `RIC` varchar(45) DEFAULT NULL,
  `CCY` varchar(45) DEFAULT NULL,
  `Side` varchar(45) DEFAULT NULL,
  `Qty` int(11) DEFAULT NULL,
  PRIMARY KEY (`SID`,`UserName`,`PermitType`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DWESchedule`
--

DROP TABLE IF EXISTS `DWESchedule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DWESchedule` (
  `SID` int(12) NOT NULL,
  `Step` int(11) NOT NULL,
  `UserName` varchar(45) NOT NULL DEFAULT 'OPEX',
  `PermitType` int(11) DEFAULT NULL,
  PRIMARY KEY (`SID`,`Step`,`UserName`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DWETimeTable`
--

DROP TABLE IF EXISTS `DWETimeTable`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DWETimeTable` (
  `SID` int(11) NOT NULL,
  `Step` int(11) NOT NULL,
  `WaitTime` double DEFAULT NULL,
  PRIMARY KEY (`SID`,`Step`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Exchange`
--

DROP TABLE IF EXISTS `Exchange`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Exchange` (
  `ExchangeName` varchar(45) NOT NULL,
  `Extension` varchar(45) NOT NULL,
  `Description` varchar(128) NOT NULL,
  PRIMARY KEY (`ExchangeName`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ExchangePhases`
--

DROP TABLE IF EXISTS `ExchangePhases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ExchangePhases` (
  `ExchangeName` varchar(45) NOT NULL,
  `StartTime` time NOT NULL,
  `EndTime` time NOT NULL,
  `Phase` varchar(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Fills`
--

DROP TABLE IF EXISTS `Fills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Fills` (
  `FillID` bigint(20) unsigned NOT NULL,
  `TimeSig` varchar(13) NOT NULL,
  `OrderID` bigint(20) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` double NOT NULL,
  `Counterparty` varchar(32) NOT NULL,
  `Instrument` varchar(32) NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`FillID`,`DateSig`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `Fillview`
--

DROP TABLE IF EXISTS `Fillview`;
/*!50001 DROP VIEW IF EXISTS `Fillview`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `Fillview` (
  `FillID` bigint(20) unsigned,
  `TimeSig` varchar(13),
  `DateSig` varchar(8),
  `Quantity` int(10) unsigned,
  `FillPrice` double,
  `Instrument` varchar(32),
  `Side` varchar(4),
  `User` varchar(45),
  `Counterparty` varchar(32),
  `LimitPrice` double
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `Instrument`
--

DROP TABLE IF EXISTS `Instrument`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Instrument` (
  `RIC` varchar(16) NOT NULL,
  `ExchangeName` varchar(45) NOT NULL,
  `MinQty` int(10) unsigned NOT NULL,
  `MaxQty` int(10) unsigned NOT NULL,
  `MinPrice` double NOT NULL,
  `MaxPrice` double NOT NULL,
  `PriceTick` double NOT NULL,
  PRIMARY KEY (`RIC`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Orders`
--

DROP TABLE IF EXISTS `Orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Orders` (
  `ClientOrderID` bigint(64) unsigned NOT NULL,
  `OrderID` bigint(64) unsigned NOT NULL,
  `ParentOrderID` bigint(64) unsigned NOT NULL,
  `Origin` varchar(64) NOT NULL,
  `Destination` varchar(64) NOT NULL,
  `Status` varchar(20) NOT NULL,
  `Instrument` varchar(16) NOT NULL,
  `Side` varchar(4) NOT NULL,
  `Currency` varchar(3) NOT NULL,
  `Type` varchar(13) NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `QuantityFilled` int(10) unsigned NOT NULL,
  `LastQuantityFilled` int(10) unsigned NOT NULL,
  `Price` double NOT NULL,
  `LastPriceFilled` double NOT NULL,
  `AveragePriceFilled` double NOT NULL,
  `TimeSig` varchar(13) NOT NULL,
  `Version` int(10) unsigned NOT NULL,
  `Message` varchar(128) NOT NULL,
  `Parameters` varchar(256) NOT NULL,
  `User` varchar(45) NOT NULL,
  `LimitPrice` double NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`OrderID`,`ClientOrderID`,`Version`,`DateSig`) USING BTREE,
  KEY `DATEINDEX` (`DateSig`,`TimeSig`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Orders';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `OrdersView`
--

DROP TABLE IF EXISTS `OrdersView`;
/*!50001 DROP VIEW IF EXISTS `OrdersView`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `OrdersView` (
  `ClientOrderID` bigint(64) unsigned,
  `OrderID` bigint(64) unsigned,
  `TimeSig` varchar(13),
  `Status` varchar(20),
  `Side` varchar(4),
  `QuantityFilled` int(10) unsigned,
  `Price` double,
  `LastPriceFilled` double,
  `Version` int(10) unsigned,
  `Message` varchar(128)
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `Shouts`
--

DROP TABLE IF EXISTS `Shouts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Shouts` (
  `ShoutID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `TimeSig` varchar(13) NOT NULL,
  `Side` varchar(5) NOT NULL,
  `Accepted` tinyint(1) NOT NULL,
  `Price` double NOT NULL,
  `User` varchar(20) NOT NULL,
  `Instrument` varchar(20) NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`ShoutID`,`DateSig`)
) ENGINE=InnoDB AUTO_INCREMENT=80910088794 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SimulationJobDescription`
--

DROP TABLE IF EXISTS `SimulationJobDescription`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SimulationJobDescription` (
  `JID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `JobName` varchar(45) NOT NULL,
  `JobDescription` text,
  PRIMARY KEY (`JID`)
) ENGINE=InnoDB AUTO_INCREMENT=1007 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SimulationJobDetails`
--

DROP TABLE IF EXISTS `SimulationJobDetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SimulationJobDetails` (
  `JID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `SID` int(10) unsigned NOT NULL,
  `SubSID` int(10) unsigned NOT NULL,
  `PID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`JID`,`SID`,`SubSID`)
) ENGINE=InnoDB AUTO_INCREMENT=1007 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SimulationJobSteps`
--

DROP TABLE IF EXISTS `SimulationJobSteps`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SimulationJobSteps` (
  `JID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `SID` int(10) unsigned NOT NULL,
  `Repetitions` int(10) unsigned NOT NULL,
  PRIMARY KEY (`JID`,`SID`)
) ENGINE=InnoDB AUTO_INCREMENT=1007 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SimulationPhases`
--

DROP TABLE IF EXISTS `SimulationPhases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SimulationPhases` (
  `PID` int(10) unsigned NOT NULL,
  `AID` int(10) unsigned NOT NULL,
  `ApplicationName` varchar(45) NOT NULL,
  `Ric` varchar(45) NOT NULL,
  `Currency` varchar(45) NOT NULL,
  `Side` varchar(45) NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` double NOT NULL,
  PRIMARY KEY (`PID`,`AID`,`ApplicationName`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SimulationSummary`
--

DROP TABLE IF EXISTS `SimulationSummary`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SimulationSummary` (
  `SimID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `PhaseID` bigint(20) unsigned NOT NULL,
  `UserName` varchar(45) NOT NULL,
  `PhaseStart` varchar(13) NOT NULL,
  `PhaseEnd` varchar(13) NOT NULL,
  `MaxThSplus` double NOT NULL,
  `EqPrice` double NOT NULL,
  `DateSig` varchar(8) NOT NULL,
  PRIMARY KEY (`SimID`,`PhaseID`,`UserName`,`DateSig`)
) ENGINE=InnoDB AUTO_INCREMENT=5761843866295488992 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;




--
-- Dumping data for table `Agents`
--

LOCK TABLES `Agents` WRITE;
/*!40000 ALTER TABLE `Agents` DISABLE KEYS */;
INSERT INTO `Agents` VALUES ('DZIP_B_1','DESZIP',1,1,1,1000,'',1000,'true','DESDemo'),('DZIP_S_1','DESZIP',1,1,1,1000,'',1000,'true','DESDemo'),('ZIP_B_1','ZIP',1,1,1,1000,'',1000,'true','AHDemo');
/*!40000 ALTER TABLE `Agents` ENABLE KEYS */;
UNLOCK TABLES;


--
-- Dumping data for table `Configuration`
--

LOCK TABLES `Configuration` WRITE;
/*!40000 ALTER TABLE `Configuration` DISABLE KEYS */;
INSERT INTO `Configuration` VALUES ('*','DBCmdTimeout','3600'),('*','DBHostName','localhost'),('*','DBPassword','password'),('*','DBPort','3306'),('*','DBSchemaName','opex'),('*','DBUserName','opex'),('*','MDSAllowedSources','OPEX'),('*','MDSAllowedSourcesHosts','localhost'),('*','OMHostLocation','localhost'),('*','SSChannelName','SupplyServiceChannel'),('*','SSHost','localhost'),('*','SSPort','12001'),('*','SSUri','SupplyService.rem'),('AHDemo','IDGeneratorType','Random'),('AHDemo','NYSESpreadImprovement','false'),('AHDemo','PurgeQueuesOnStartup','false'),('GUIDemo','IDGeneratorType','Random'),('GUIDemo','PurgeQueuesOnStartup','false'),('OPEX','AllowAmendments','true'),('OPEX','AllowCancellations','true'),('OPEX','ClosePeriodDuration','00:00:10'),('OPEX','MDSDataSource','OPEX'),('OPEX','NYSESpreadImprovement','false'),('OPEX','OpenPeriodDuration','00:02:50'),('OPEX','PurgeQueuesOnStartup','true'),('OrderManager','OMAllowedDestinations','OPEX'),('OrderManager','OMAllowedDestinationsHosts','localhost'),('OrderManager','OMMode','Server'),('OrderManager','PurgeQueuesOnStartup','true'),('SELLER1','IDGeneratorType','Random'),('SELLER1','PurgeQueuesOnStartup','false');
/*!40000 ALTER TABLE `Configuration` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `DESJobs`
--

LOCK TABLES `DESJobs` WRITE;
/*!40000 ALTER TABLE `DESJobs` DISABLE KEYS */;
INSERT INTO `DESJobs` VALUES (1234,10);
/*!40000 ALTER TABLE `DESJobs` ENABLE KEYS */;
UNLOCK TABLES;


--
-- Dumping data for table `DWEPermits`
--

LOCK TABLES `DWEPermits` WRITE;
/*!40000 ALTER TABLE `DWEPermits` DISABLE KEYS */;
INSERT INTO `DWEPermits` VALUES (1,'ZIP_B_1',1,210,'TTECH.L','GBp','Buy',1),(1,'ZIP_B_1',2,200,'TTECH.L','GBp','Buy',1),(1,'ZIP_B_1',3,190,'TTECH.L','GBp','Buy',1),(1,'SELLER1',1,190,'TTECH.L','GBp','Sell',1),(1,'SELLER1',2,200,'TTECH.L','GBp','Sell',1),(1,'SELLER1',3,210,'TTECH.L','GBp','Sell',1);
/*!40000 ALTER TABLE `DWEPermits` ENABLE KEYS */;
UNLOCK TABLES;


--
-- Dumping data for table `DWESchedule`
--

LOCK TABLES `DWESchedule` WRITE;
/*!40000 ALTER TABLE `DWESchedule` DISABLE KEYS */;
INSERT INTO `DWESchedule` VALUES (1,0,'ZIP_B_1',1),(1,0,'SELLER1',1),(1,1,'ZIP_B_1',2),(1,1,'SELLER1',2),(1,2,'ZIP_B_1',3),(1,2,'SELLER1',3);
/*!40000 ALTER TABLE `DWESchedule` ENABLE KEYS */;
UNLOCK TABLES;


--
-- Dumping data for table `DWETimeTable`
--

LOCK TABLES `DWETimeTable` WRITE;
/*!40000 ALTER TABLE `DWETimeTable` DISABLE KEYS */;
INSERT INTO `DWETimeTable` VALUES (1,0,10),(1,1,10),(1,2,10);
/*!40000 ALTER TABLE `DWETimeTable` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `Instrument`
--

LOCK TABLES `Instrument` WRITE;
/*!40000 ALTER TABLE `Instrument` DISABLE KEYS */;
INSERT INTO `Instrument` VALUES ('BARC.L','OPEX',1,1000000,1,400,1),('LLOY.L','OPEX',1,1000000,1,400,1),('RBS.L','OPEX',1,1000000,1,400,1),('TSCO.L','OPEX',1,1000000,1,400,1),('TTECH.L','OPEX',1,1000000,1,400,1),('VOD.L','OPEX',1,1000000,1,400,1);
/*!40000 ALTER TABLE `Instrument` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `SimulationJobDescription`
--

LOCK TABLES `SimulationJobDescription` WRITE;
/*!40000 ALTER TABLE `SimulationJobDescription` DISABLE KEYS */;
INSERT INTO `SimulationJobDescription` VALUES (1001,'Demo','Preset Demo SimulationJob'),(1234,'DemoDES','Preset Demo SimulationJob for DES');
/*!40000 ALTER TABLE `SimulationJobDescription` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `SimulationJobDetails`
--

LOCK TABLES `SimulationJobDetails` WRITE;
/*!40000 ALTER TABLE `SimulationJobDetails` DISABLE KEYS */;
INSERT INTO `SimulationJobDetails` VALUES (1001,10001,0,100001),(1234,20001,0,200001);
/*!40000 ALTER TABLE `SimulationJobDetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `SimulationJobSteps`
--

LOCK TABLES `SimulationJobSteps` WRITE;
/*!40000 ALTER TABLE `SimulationJobSteps` DISABLE KEYS */;
INSERT INTO `SimulationJobSteps` VALUES (1001,10001,3),(1234,20001,10);
/*!40000 ALTER TABLE `SimulationJobSteps` ENABLE KEYS */;
UNLOCK TABLES;


--
-- Dumping data for table `SimulationPhases`
--

LOCK TABLES `SimulationPhases` WRITE;
/*!40000 ALTER TABLE `SimulationPhases` DISABLE KEYS */;
INSERT INTO `SimulationPhases` VALUES (100001,0,'SELLER1','TTECH.L','GBp','Sell',1,150),(100001,0,'ZIP_B_1','TTECH.L','GBp','Buy',1,250),(100001,1,'SELLER1','TTECH.L','GBp','Sell',1,175),(100001,1,'ZIP_B_1','TTECH.L','GBp','Buy',1,225),(100001,2,'SELLER1','TTECH.L','GBp','Sell',1,200),(100001,2,'ZIP_B_1','TTECH.L','GBp','Buy',1,200),(100001,3,'SELLER1','TTECH.L','GBp','Sell',1,225),(100001,3,'ZIP_B_1','TTECH.L','GBp','Buy',1,175),(100001,4,'SELLER1','TTECH.L','GBp','Sell',1,250),(100001,4,'ZIP_B_1','TTECH.L','GBp','Buy',1,150),(200001,0,'DZIP_B_1','TTECH.L','GBp','Buy',1,250),(200001,0,'DZIP_S_1','TTECH.L','GBp','Sell',1,150),(200001,1,'DZIP_B_1','TTECH.L','GBp','Buy',1,225),(200001,1,'DZIP_S_1','TTECH.L','GBp','Sell',1,175),(200001,2,'DZIP_B_1','TTECH.L','GBp','Buy',1,200),(200001,2,'DZIP_S_1','TTECH.L','GBp','Sell',1,200),(200001,3,'DZIP_B_1','TTECH.L','GBp','Buy',1,175),(200001,3,'DZIP_S_1','TTECH.L','GBp','Sell',1,225),(200001,4,'DSELLER1','TTECH.L','GBp','Sell',1,250),(200001,4,'DZIP_B_1','TTECH.L','GBp','Buy',1,150);
/*!40000 ALTER TABLE `SimulationPhases` ENABLE KEYS */;
UNLOCK TABLES;


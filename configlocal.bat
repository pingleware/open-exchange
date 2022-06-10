
if exist C:\OPEX\Config\OPEX.ConfigurationServer.exe.config  xcopy /y C:\OPEX\Config\OPEX.ConfigurationServer.exe.config C:\OPEX\CS
if exist C:\OPEX\Config\OPEX.ConsoleExchange.exe.config	     xcopy /y C:\OPEX\Config\OPEX.ConsoleExchange.exe.config C:\OPEX\ME
if exist C:\OPEX\Config\OPEX.ConsoleOrderManager.exe.config  xcopy /y C:\OPEX\Config\OPEX.ConsoleOrderManager.exe.config C:\OPEX\OM
if exist C:\OPEX\Config\TradingGUI.exe.config		     xcopy /y C:\OPEX\Config\TradingGUI.exe.config C:\OPEX\GUI
if exist C:\OPEX\Config\OPEX.SalesGUI.exe.config	     xcopy /y C:\OPEX\Config\OPEX.SalesGUI.exe.config  C:\OPEX\TTGUI
if exist C:\OPEX\Config\OPEX.SupplyServer.exe.config	     xcopy /y C:\OPEX\Config\OPEX.SupplyServer.exe.config C:\OPEX\SS
if exist C:\OPEX\Config\OPEX.SupplyService.SSGUI.exe.config  xcopy /y C:\OPEX\Config\OPEX.SupplyService.SSGUI.exe.config C:\OPEX\SSGUI
pause

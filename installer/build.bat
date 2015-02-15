echo "Welcome to lenovo wifi build console"

%VS120COMNTOOLS%\vsDevCmd.bat

msbuild "..\LenovoWiFi.sln" /p:Configuration=Release /p:Platform=Win32
msbuild "..\LenovoWiFi.sln" /p:Configuration=Release /p:Platform=x64

del ..\..\output\* /S /Q
md ..\..\output
md ..\..\rel_inst

xcopy * ..\..\output\
xcopy ..\bin\Release\* ..\..\output\ /S

"C:\Program Files (x86)\Inno Setup 5\Compil32.exe" /cc "..\..\output\LenovoWifiInstaller.iss"

set dst=%date:~0,4%%date:~5,2%%date:~8,2%
del ..\..\rel_inst\%dst%\* /S /Q
md ..\..\rel_inst\%dst%
move ..\..\output\LenovoWifiSetup.exe ..\..\rel_inst\%dst%\lenovowifi%dst%.exe

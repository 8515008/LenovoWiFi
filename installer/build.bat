echo "Welcome to lenovo wifi build console"

;msbuild "xxx.sln" /p:Configuration=Release /p:Platform=x86

;C:\Users\james\Documents\GitHub\LenovoWiFi

del ..\..\output\* /S /Q
md ..\..\output

copy * ..\..\output\
copy ..\bin\Release\* ..\..\output\
copy ..\bin\Release\x64\* ..\..\output\
"C:\Program Files (x86)\Inno Setup 5\Compil32.exe" /cc "..\..\output\LenovoWifiInstaller.iss"
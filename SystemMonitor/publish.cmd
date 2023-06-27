REM From https://dotnetcoretutorials.com/2021/11/10/single-file-apps-in-net-6/

dotnet publish -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -r win-x64 -c Release --self-contained true /p:AssemblyVersion=0.3.2 /p:Version=0.3.2

start bin\Release\net6.0-windows\win-x64\publish
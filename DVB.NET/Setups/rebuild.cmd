set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

rd /s /q ..\Assemblies

%msbuild% "Core\Core.sln" /p:Configuration=Release;Platform="Mixed Platforms" /v:minimal /t:Clean;Build /m /nr:false /fl /flp:Verbosity=Normal;LogFile="Core\Build.log"
%msbuild% "Server\Server.sln" /p:Configuration=Release;Platform="Mixed Platforms" /v:minimal /t:Clean;Build /m /nr:false /fl /flp:Verbosity=Normal;LogFile="Server\Build.log"
%msbuild% "Tools\Tools.sln" /p:Configuration=Release;Platform="Mixed Platforms" /v:minimal /t:Clean;Build /m /nr:false /fl /flp:Verbosity=Normal;LogFile="Tools\Build.log"

pause

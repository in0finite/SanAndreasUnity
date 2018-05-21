@echo off
SonarQube\SonarScanner.MSBuild.exe begin /k:"sanandreasunity" /d:sonar.organization="z3nth10n-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="86ca44fe46bfec2ee42260696bf821d7891d2a03"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MsBuild.exe" "SanAndreasUnity.sln" /t:Rebuild
SonarQube\SonarScanner.MSBuild.exe end /d:sonar.login="86ca44fe46bfec2ee42260696bf821d7891d2a03"
pause
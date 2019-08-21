$RunDir = Split-Path -Parent $MyInvocation.MyCommand.Definition;
#Build���빤��
msbuild BuildTools\Source\BuildFromGithub\BuildFromGithub.csproj /t:Build /p:Configuration=Release /p:TargetFramework=v4.5
mkdir tmp/soucre_github
#��������
git clone https://github.com/chawolbaka/MinecraftProtocol.git tmp/soucre_github
cp -r "BuildFiles\packages\" "tmp"
.\BuildTools\BuildFromGithub.exe "$RunDir\tmp\soucre_github\MinecraftProtocol" "$RunDir\BuildFiles\MinecraftProtocol" "$RunDir\tmp\Build" "$RunDir\BuildFiles\MinecraftProtocol\MinecraftProtocol.csproj"
msbuild tmp\Build\MinecraftProtocol.csproj /t:Build /p:Configuration=Release /p:TargetFramework=v4.5
#��ʼ���뱾��
mkdir bin
cp tmp\Build\bin\Release\MinecraftProtocol.dll bin
Remove-Item -Path .\tmp\Build -Recurse -Force
Remove-Item -Path .\tmp\soucre_github -Recurse -Force
mkdir .\tmp\soucre_github
git clone https://github.com/chawolbaka/PlayersMonitor.git tmp/soucre_github
.\BuildTools\BuildFromGithub.exe "$RunDir\tmp\soucre_github\PlayersMonitor-Console" "$RunDir\BuildFiles\PlayersMonitor" "$RunDir\tmp\Build" "$RunDir\BuildFiles\PlayersMonitor\PlayersMonitor.csproj"
#���Ƹոձ���������ļ���ȥ
cp .\bin\MinecraftProtocol.dll  .\tmp\Build\Dependence\
msbuild .\tmp\Build\PlayersMonitor.csproj  /p:Configuration=Release
#�������,��ʼ�����ļ�&ȡ������õ��ļ�
cp -r .\tmp\Build\bin\Release\*  .\bin
Remove-Item -Path .\tmp -Recurse -Force

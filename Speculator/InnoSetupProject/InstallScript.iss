#define MyAppName     "ZX Speculator"
#define MyAppExeName  "Speculator.exe"

[Setup]
AppId={{996B4B28-314A-451F-ED15-8777E28DBDE4}
AppName={#MyAppName}
AppVersion=2.1
AppPublisher=Dean Edis
AppPublisherURL=https://github.com/deanthecoder/ZXSpeculator
DefaultDirName={commonpf}\ZXSpeculator
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\Inno_Setup_Project.exe
Compression=lzma2
SolidCompression=yes
SourceDir=..\Speculator\
OutputDir=..\InnoSetupProject\
OutputBaseFilename={#MyAppName} Installer

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net7.0\publish\win-x64\*.*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion
Source: "bin\Release\net7.0\publish\win-x64\ROMs\*.*"; DestDir: "{app}\ROMs"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; Waraq for Windows — Inno Setup 6 (WRQ-WIN-002 Phase 9)
; Builds the current WinUI 3 app under src/ (not archive MVP).

#ifndef MyAppVersion
  #define MyAppVersion "0.9.0-phase9"
#endif
#ifndef MyAppSourceDir
  #define MyAppSourceDir "..\artifacts\publish"
#endif
#ifndef MyOutputDir
  #define MyOutputDir "..\artifacts\installer"
#endif
#ifndef MyOutputBase
  #define MyOutputBase "Waraq.Windows-Setup-win-x64-" + MyAppVersion
#endif

#define MyAppName "Waraq for Windows"
#define MyAppPublisher "Waraq Windows contributors"
#define MyAppURL "https://github.com/taroo0ooq/waraq-windows"
#define MyAppExeName "Waraq.Windows.App.exe"

[Setup]
AppId={{B7E4D2C1-9A80-4F31-8E2B-6C5D4A392817}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={localappdata}\Programs\WaraqWindows
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
InfoBeforeFile=..\docs\install\WINDOWS-INSTALLER-NOTES.txt
OutputDir={#MyOutputDir}
OutputBaseFilename={#MyOutputBase}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion=0.9.0.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup (WinUI)
VersionInfoProductName={#MyAppName}
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Self-contained publish (NET 8 + Windows App SDK self-contained)
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\NOTICE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\install\WINDOWS.md"; DestDir: "{app}"; DestName: "README-INSTALL.md"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Prereq strategy: self-contained publish bundles .NET 8 + WASDK (ADR 0002 / Phase 9).
// No third-party mirrors. Official Microsoft URLs only if FD bootstrap is added later.

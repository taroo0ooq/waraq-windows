; Waraq for Windows — Inno Setup 6 installer (WRQ-WIN-001 Phase 6)
; Built by windows/scripts/Build-Installer.ps1 and CI windows-release.

#ifndef MyAppVersion
  #define MyAppVersion "0.2.0-alpha"
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
#define MyAppExeName "Waraq.Windows.exe"

[Setup]
AppId={{8F3C2A91-6B4E-4D27-9C1A-7E5D4B3A2100}
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
VersionInfoVersion=0.2.0.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Self-contained publish tree (includes bundled .NET runtime)
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
// Placeholder for future official vendor bootstrap if payload switches to framework-dependent.
// Phase 6 prereq strategy: self-contained publish bundles .NET 8 (see ADR 0002).

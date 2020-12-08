; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
#define AppIcon "Icon.ico"
#define AppExeFile "HSMServer.exe"
#define AppShortcutName "HSMServer"
#define ReleaseFilesPath "..\HSMServer\bin\Release\netcoreapp3.1\publish"

[Setup]
AppId={{4BC0F231-A95E-44F7-AE9B-25A1A4989B64}
AppName=HSMServer
AppVerName=HSMServer 0.8
DefaultDirName={pf}\HSMServer
DefaultGroupName=HSMServer
CreateAppDir=yes
DisableDirPage=no
AppPublisher=Soft-FX

;InfoBeforeFile=
OutputBaseFilename=HSMServer_setup_0.8
;SetupIconFile=
;UninstallDisplayIcon=
DisableProgramGroupPage=yes
;LicenseFile=
Compression=lzma
SolidCompression=yes
;WizardImageFile=
;WizardSmallImageFile=

[Languages]
;
;

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Dirs]
;Name: "{app}\Data"
;Name: "{app}\Help"
;Name: "{app}"; Attribs: system

[Files]
Source: "{#ReleaseFilesPath}\{#AppExeFile}"; DestDir: "{app}"; Flags: ignoreversion
;Source: "{#ReleaseFilesPath}\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#ReleaseFilesPath}\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppIcon}"; DestDir: "{app}"; Flags: ignoreversion

[UninstallDelete]
;

[Icons]
;
;Name: "{app}\HSMClient"; Filename: "{app}\{#AppExeFile}"; IconFilename: "{app}\{#AppIcon}"; Tasks: desktopicon 
Name: "{userdesktop}\{#AppShortcutName}"; Filename: "{app}\{#AppExeFile}"; IconFilename: "{app}\{#AppIcon}"; Tasks: desktopicon 

[Run]
Filename: "{app}\{#AppExeFile}"; Description: "Launch HSM server"; Flags: nowait postinstall
;
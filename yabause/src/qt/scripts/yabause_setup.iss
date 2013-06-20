; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{1EE50957-0C07-4B9B-ADA5-D81C7F663EC9}
AppName=Yabause Qt Gui
AppVerName=Yabause Qt Gui 0.9.4
AppPublisher=Yabause Team
AppPublisherURL=http://www.yabause.org
AppSupportURL=http://www.yabause.org
AppUpdatesURL=http://www.yabause.org
DefaultDirName={pf}\Yabause Qt Gui
DefaultGroupName=Yabause Qt Gui
LicenseFile=Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\COPYING
InfoBeforeFile=Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\README.QT
InfoAfterFile=Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\ChangeLog
OutputBaseFilename=setup_yabause_qt_0.9.4
Compression=lzma
SolidCompression=yes

[Languages]
Name: english; MessagesFile: compiler:Default.isl
Name: french; MessagesFile: compiler:Languages\French.isl
Name: portuguese; MessagesFile: compiler:Languages\Portuguese.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\yabause.exe; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\SDL.dll; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\AUTHORS; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\ChangeLog; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\COPYING; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\mingwm10.dll; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\QtCore4.dll; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\QtGui4.dll; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\QtOpenGL4.dll; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\README; DestDir: {app}; Flags: ignoreversion
Source: Z:\Documents\Development\Qt4\yabause-cvs\yabause\src\qt\win32\README.QT; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Yabause Qt Gui; Filename: {app}\yabause.exe
Name: {commondesktop}\Yabause Qt Gui; Filename: {app}\yabause.exe; Tasks: desktopicon
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Yabause Qt Gui; Filename: {app}\yabause.exe; Tasks: quicklaunchicon

[Run]
Filename: {app}\yabause.exe; Description: {cm:LaunchProgram,Yabause Qt Gui}; Flags: nowait postinstall skipifsilent


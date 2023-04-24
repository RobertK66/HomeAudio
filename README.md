# HomeAudio
Simple WebRadio/CD-Player based on [Sharpcaster](https://github.com/Tapanila/SharpCaster/) and [ConsoleGUI](https://github.com/TomaszRewak/C-sharp-console-gui-framework)

Usage:
* Download latest release
* Extract version for Windows or Linux to your working dir.
  * Change the "CcName" entry in appsettings.json to give the name of the chromecast device you want to use for playback (if not sure what the name is look for '... ChromeCastWrapper - found \<xyz\>' messages in log panel).
  * Edit the files 'Cds.json' and 'WebRadios.json' to your needs.
  * If needed adapt the Section "MediaTabs" in appsettings.json (Resize Cols, Rows and CellSize or add/remove more Tabs)
  * Execute ConGui(.exe) application and have fun.
  * Note: There is no need to keep the App running after a CD is queued. The cc-device handles the queue!


Keyboard usage:

Key | Function 
--- | --- 
'-' | Volume down 
'+' | Volume up
\<tab\> | Select next media tab
\<left, right, up, down\> | Select media entry
\<Enter\> | Start Playing
\<End\> | Next Track (CDs only)
\<Pos1\> | Previous Track (CDs only)
\<PgUp\> | Scroll in Log-Panel (stop live scroll)
\<PgDown\> | Scroll in Log-Panel (start live scrolll if End is reached)
 
  
  

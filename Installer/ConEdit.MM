;----------------------------------------------------------------------------
;    MODULE NAME:   CONEDIT.MM
;
;        $Author:   USER "Dennis"  $
;      $Revision:   1.15  $
;          $Date:   02 Jun 2006 17:10:46  $
;       $Logfile:   C:/DBAREIS/Projects.PVCS/Win32/MakeMsi/ConEdit.mm.pvcs  $
;
; DESCRIPTION
; ~~~~~~~~~~~
; This is a simple sample/test MSI. Takes about 30 seconds to build and
; validate on my AMD 3200.
;
; Any line within this file that begins with ";" can be ignored as its
; only a comment so there are only 3 important lines in this file:
;
;   1. #include "ME.MMH"
;   2. <$DirectoryTree Key="INSTALLDIR" ...
;   3. <$Files "ConEdit.*" DestDir="INSTALLDIR">
;----------------------------------------------------------------------------


;--- Include MAKEMSI support (with my customisations and MSI branding) ------
#define VER_FILENAME.VER  ConEdit.Ver      ;;I only want one VER file for all samples! (this line not actually required in "conedit.mm")
#include "ME.MMH"


;--- Want to debug (not common) ---------------------------------------------
;;#debug on
;#Option DebugLevel=^NONE, +OpSys^


;--- Define default location where file should install and add files --------
<$DirectoryTree Key="INSTALLDIR" Dir="c:\program files\Shanes Conversion Editor" CHANGE="\" PrimaryFolder="Y">
<$Files "C:\Users\shane\Desktop\ConEdit\ConversationEditor.exe" DestDir="INSTALLDIR">
<$Files "C:\Users\shane\Desktop\ConEdit\*.dll" DestDir="INSTALLDIR">

<$DirectoryTree Key="PLUGINS" Dir="c:\program files\Shanes Conversion Editor\Plugins" CHANGE="\" PrimaryFolder="Y">
<$Files "C:\Users\shane\Desktop\ConEdit\Plugins\*.dll" DestDir="PLUGINS">       ;;The installed MAKEMSI files

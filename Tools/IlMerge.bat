REM copy CodeTV.exe CodeTV.bak.exe
ilmerge /target:winexe /out:..\CodeTV.exe CodeTV.exe DirectShowLib-2005.dll WeifenLuo.WinFormsUI.Docking.dll
move ..\CodeTV.exe CodeTV.exe
del DirectShowLib-2005.dll WeifenLuo.WinFormsUI.Docking.dll
pause

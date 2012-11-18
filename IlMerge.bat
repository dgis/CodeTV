ilmerge /target:winexe /out:CodeTV.exe bin\Release\CodeTV.exe bin\Release\DvbPsi.dll bin\Release\DirectShowLib-2005.dll bin\Release\MediaFoundation.dll bin\Release\WeifenLuo.WinFormsUI.Docking.dll
ilmerge /target:winexe /out:"CodeTV x86.exe" bin\Release\CodeTV.exe bin\x86\Release\DvbPsi.dll bin\x86\Release\DirectShowLib-2005.dll bin\x86\Release\MediaFoundation.dll bin\x86\Release\WeifenLuo.WinFormsUI.Docking.dll
pause

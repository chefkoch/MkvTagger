
REM Select program path based on current machine environment
REM set progpath=
REM if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

cd ..\Source\bin\x86\Release

del *.pdb
del *.xml
del *.7z
del *.vshost.*

"C:\Program Files\7-Zip\7z.exe" a -mx9 -t7z ..\..\..\..\Release.7z *
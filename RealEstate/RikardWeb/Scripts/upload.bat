call stop-service.bat

del /Q dist.tgz
7z.exe a -ttar -so dist.tar dist | 7z.exe a -si dist.tgz
winscp /script=upload.wscp

call start-service.bat

call 0_Setup.bat

%SIGNToolPath%makecert -a SHA256 -sv %SRCDRV%\%KeyName%.pvk -n "CN=DI Herbert Aitenbichler" %SRCDRV%\%KeyName%.cer -r
%SIGNToolPath%pvk2pfx -pvk %SRCDRV%\%KeyName%.pvk -spc %SRCDRV%\%KeyName%.cer -pfx %SRCDRV%\%KeyName%.pfx -po "%1"

pause
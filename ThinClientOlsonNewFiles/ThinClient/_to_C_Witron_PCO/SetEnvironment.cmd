echo a > NUL && @set NUL=NUL 2^>^&1
echo a > NUL || @set NUL=%temp%\null 2^>^&1

SET APP_SERVER=RFL6DPSPW2C
SET APPL_RUNTIME_SHARE=CVS-APP-RT
SET APPL_VAR_SHARE=CVS-APP-VAR
SET APPL_LOG_SHARE=CVS-CLIENT-LOG
SET WS_USER_AUTOLOGON=WCLIENT_USER
SET WS_PASSWORD=WMS.2016
SET CLIENT_WITRON_DIR=%PROGRAMFILES%\Witron
SET LOGON_SERVER=RFL6DPSPW2C
SET LOGON_SERVER_DOMAIN=RFL6WMSDOM
SET LOGON_SCRIPT=\\%LOGON_SERVER%\CVS-APP-RT\atl\runtime\etc\ClientFiles\LogonScript.bat
SET LOGON_LOG=\\%LOGON_SERVER%\CVS-CLIENT-LOG\Client_Startup\
SET LOGON_RUNTIME_SHARE=CVS-APP-RT
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT APP_SERVER.WitronClientSetup" APP_SERVER
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT APPL_RUNTIME_SHARE.WitronClientSetup" APPL_RUNTIME_SHARE
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT APPL_VAR_SHARE.WitronClientSetup" APPL_VAR_SHARE
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT APPL_LOG_SHARE.WitronClientSetup" APPL_LOG_SHARE> %NUL%
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT CLIENT_WITRON_DIR.WitronClientSetup" CLIENT_WITRON_DIR
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT WS_USER_AUTOLOGON.WitronClientSetup" WS_USER_AUTOLOGON
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT WS_PASSWORD.WitronClientSetup" WS_PASSWORD
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_SERVER.WitronClientSetup" LOGON_SERVER
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_SERVER_DOMAIN.WitronClientSetup" LOGON_SERVER_DOMAIN
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_SERVER_USER.WitronClientSetup" LOGON_SERVER_USER
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_SERVER_PASSWORD.WitronClientSetup" LOGON_SERVER_PASSWORD > %NUL%
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_SCRIPT.WitronClientSetup" LOGON_SCRIPT
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_LOG.WitronClientSetup" LOGON_LOG
CALL %0\..\ScriptHelperToEnv.cmd "QUERYDNSTEXT LOGON_RUNTIME_SHARE.WitronClientSetup" LOGON_RUNTIME_SHARE
@ECHO OFF

REM Derive a set of classes from the schema file for use when loading the HSM
REM inputs from the XML configuration file.

SETLOCAL

SET FNAME=hsm-inputs
SET CFG_DIR=%~dp0..\hsm\configuration.service
SET SCHEMA_DIR=%CFG_DIR%\Resources
SET INPUT_SCHEMA=%SCHEMA_DIR%\%FNAME%.xsd

xsd %INPUT_SCHEMA% /classes /namespace:OCC.HSM.Model /language:CS /out:%~dp0
IF ERRORLEVEL 1 GOTO END

unexpand --tabs=4 %~dp0%FNAME%.cs > %~dp0%FNAME%.tabbed
IF ERRORLEVEL 1 GOTO END

copy /y %~dp0%FNAME%.tabbed %~dp0..\hsm\model\InputsType.gen.cs
IF ERRORLEVEL 1 GOTO END

awk '/private .*(Field);/{if($2 ~ /\[\]/) exit;print "\t\t\t/// <summary>\n\t\t\t/// Get the question for the",^
	$2, "\n\t\t\t/// </summary>\n\t\t\tQuestion",^
	$2, "{ get; }\n"}' %~dp0%FNAME%.cs | sed -e "s/( /(/" -e "s/; )/);/"

:END
del %~dp0%FNAME%.cs
del %~dp0%FNAME%.tabbed

ENDLOCAL
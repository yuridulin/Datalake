@echo off
REM Список необходимых environment переменных
set DATALAKE_HOST=
set DATALAKE_DB=
set DATALAKE_DB_USER=
set DATALAKE_DB_PASS=

REM Функция для проверки и установки environment переменных
call :set_env DATALAKE_HOST
call :set_env DATALAKE_DB
call :set_env DATALAKE_DB_USER
call :set_env DATALAKE_DB_PASS

REM Завершение скрипта
echo All environment variables have been set.
pause
exit /b

REM Функция для проверки и установки environment переменных
:set_env
setlocal
set "var_name=%~1"
for /f "tokens=3" %%a in ('reg query "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v %var_name% 2^>nul') do set "current_value=%%a"
if defined current_value (
    set /p "new_value=Enter value for %var_name% (current value: %current_value%): "
) else (
    set /p "new_value=Enter value for %var_name%: "
)

if not defined new_value (
    set "new_value=%current_value%"
)

if not defined new_value (
    echo Error: %var_name% must be set.
    exit /b 1
)

echo Setting %var_name% to %new_value%
setx %var_name% "%new_value%" -m >nul
endlocal
exit /b
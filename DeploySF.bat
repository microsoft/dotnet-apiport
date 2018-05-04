REM sfctl cluster select --endpoint https://portabilityservice.eastus.cloudapp.azure.com:19080 --pem "C:\Users\miker\Downloads\portabilityservice-PortabilityServiceSF-20180430.pem" --no-verify
sfctl application upload --path .\src\backend\PortabilityService.ServiceFabricApplication
sfctl application provision --application-type-build-path PortabilityService.ServiceFabricApplication
sfctl application create --app-name fabric:/PortabilityService --app-type PortabilityServiceType --app-version v1
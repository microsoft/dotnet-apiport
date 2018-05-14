Manual SeaBreeze Deployment Instructions
========================================

Based on docs [here](https://github.com/Azure/seabreeze-preview-pr/blob/master/docs/conceptual-docs/application-deployment-quickstart.md).

1. Install [SF Mesh CLI](https://github.com/Azure/service-fabric-mesh-preview-pr/blob/private-preview_3/docs/conceptual-docs/cli-setup.md).
1. `az login`
1. `az account set --subscription "<subscriptionName>"`
1. `az mesh deployment create --resource-group <resourceGroupName> --template-file sampleapp.json --verbose`
    - Or `az mesh deployment create --resource-group portabilityservice-mesh-1709 --template-file PortabilityService-1709.json --verbose`
    - Or `az mesh deployment create --resource-group portabilityservice-mesh-linux --template-file PortabilityService-Linux.json --verbose`
1. `az mesh app list -o table`
    1. Confirm the app was created successfully.
1. `az mesh network list`
    1. This will show the IP address where the app's endpoint is exposed.
1. `az mesh codepackage logs --resource-group portabilityservice-mesh-1709 --application-name PortabilityService --service-name Gateway --replica-name 0 --name Gateway.Code`
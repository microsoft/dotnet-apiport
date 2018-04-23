Manual SeaBreeze Deployment Instructions
========================================

Based on docs [here](https://github.com/Azure/seabreeze-preview-pr/blob/master/docs/conceptual-docs/application-deployment-quickstart.md).

1. Install [SeaBreeze CLI](https://github.com/Azure/seabreeze-preview-pr/blob/master/docs/conceptual-docs/cli-setup.md).
1. `az login`
1. `az account set --subscription "<subscriptionName>"`
1. `az sbz deployment create --resource-group <resourceGroupName> --template-file sampleapp.json`
1. `az sbz app list -o table`
    1. Confirm the app was created successfully.
1. `az sbz network list`
    1. This will show the IP address where the app's endpoint is exposed.
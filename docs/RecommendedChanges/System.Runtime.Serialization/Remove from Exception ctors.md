### Recommended Action
Either 1) Delete Serialization info from exceptions (since this can't be remoted) or 2) Use a different serialization technology if not for exceptions.

### Affected APIs
* `T:System.Runtime.Serialization.ISafeSerializationData`
* `T:System.Runtime.Serialization.SafeSerializationEventArgs`
* `T:System.Runtime.Serialization.SerializationInfo`

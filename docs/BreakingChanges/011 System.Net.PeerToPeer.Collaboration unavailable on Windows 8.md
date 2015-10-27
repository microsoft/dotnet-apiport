## 11: System.Net.PeerToPeer.Collaboration unavailable on Windows 8

### Scope
Major

### Version Introduced
4.5

### Source Analyzer Status
Available

### Change Description
The System.Net.PeerToPeer.Collaboration namespace is unavailable on Windows 8 or above.

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
Apps that support Windows 8 or above must be updated to not depend on this namespace or its members.

### Affected APIs
* `T:System.Net.PeerToPeer.Collaboration.ContactManager`
* `T:System.Net.PeerToPeer.Collaboration.CreateContactCompletedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.InviteCompletedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.NameChangedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.ObjectChangedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.Peer`
* `T:System.Net.PeerToPeer.Collaboration.PeerApplication`
* `T:System.Net.PeerToPeer.Collaboration.PeerApplicationCollection`
* `T:System.Net.PeerToPeer.Collaboration.PeerApplicationLaunchInfo`
* `T:System.Net.PeerToPeer.Collaboration.PeerApplicationRegistrationType`
* `T:System.Net.PeerToPeer.Collaboration.PeerChangeType`
* `T:System.Net.PeerToPeer.Collaboration.PeerCollaboration`
* `T:System.Net.PeerToPeer.Collaboration.PeerCollaborationPermission`
* `T:System.Net.PeerToPeer.Collaboration.PeerCollaborationPermissionAttribute`
* `T:System.Net.PeerToPeer.Collaboration.PeerContact`
* `T:System.Net.PeerToPeer.Collaboration.PeerContactCollection`
* `T:System.Net.PeerToPeer.Collaboration.PeerEndPoint`
* `T:System.Net.PeerToPeer.Collaboration.PeerEndPointCollection`
* `T:System.Net.PeerToPeer.Collaboration.PeerInvitationResponse`
* `T:System.Net.PeerToPeer.Collaboration.PeerInvitationResponseType`
* `T:System.Net.PeerToPeer.Collaboration.PeerNearMe`
* `T:System.Net.PeerToPeer.Collaboration.PeerNearMeChangedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.PeerNearMeCollection`
* `T:System.Net.PeerToPeer.Collaboration.PeerObject`
* `T:System.Net.PeerToPeer.Collaboration.PeerObjectCollection`
* `T:System.Net.PeerToPeer.Collaboration.PeerPresenceInfo`
* `T:System.Net.PeerToPeer.Collaboration.PeerPresenceStatus`
* `T:System.Net.PeerToPeer.Collaboration.PeerScope`
* `T:System.Net.PeerToPeer.Collaboration.PresenceChangedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.RefreshDataCompletedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.SubscribeCompletedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.SubscriptionListChangedEventArgs`
* `T:System.Net.PeerToPeer.Collaboration.SubscriptionType`

[More information](https://msdn.microsoft.com/en-us/library/hh367887#network)

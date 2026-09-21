type: feature

All four packages declare `IsTrimmable` and `IsAotCompatible` on net8.0 and later, so trimmed and native-AOT consumers no longer get IL2104-style "assembly was not verified" warnings for them; the shipped code carries no trim or AOT diagnostics.

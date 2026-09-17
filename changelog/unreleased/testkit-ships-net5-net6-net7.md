type: feature

`Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit` ship `net5.0`, `net6.0` and `net7.0` assemblies, so an inherited init-only property written from either package resolves the `IsExternalInit` modreq against the matching Abstractions asset instead of throwing `MissingMethodException` on .NET 5–7.

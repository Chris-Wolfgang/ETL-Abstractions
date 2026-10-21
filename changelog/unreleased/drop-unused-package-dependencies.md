type: fix

`Wolfgang.Etl.Abstractions` no longer depends on `System.ComponentModel.Annotations` and no longer packs `ETL-Abstractions.ico` into the package `content/` folder, and `Wolfgang.Etl.TestKit` / `Wolfgang.Etl.TestKit.Xunit` no longer depend on `Microsoft.Bcl.Memory`; none of them used these.

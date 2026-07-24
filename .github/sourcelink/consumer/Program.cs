using System;
using Wolfgang.Etl.Abstractions;

// End-to-end SourceLink "step into" fixture. The debugger sets a breakpoint on
// the marked line below and issues a step-into (the F11 a consumer
// would press). If SourceLink is intact the debugger resolves the library's real
// source (from GitHub) at the constructor below, instead of a decompiled
// placeholder. Report's constructor is a plain, non-async method with a guard
// clause, which makes it a clean and stable step-into target.

var report = new Report(42); // STEP_INTO_TARGET
Console.WriteLine(report.CurrentItemCount);

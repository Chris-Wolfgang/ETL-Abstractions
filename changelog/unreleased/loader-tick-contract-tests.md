type: fix

The two `LoaderBaseContractTests` "invokes callback when timer fires" tests now check the callback before the load completes, so they fail when a timer tick delivers no report instead of being satisfied by the final report.

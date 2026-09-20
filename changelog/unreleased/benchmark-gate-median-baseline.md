type: internal

The PR benchmark gate compares against the median of the last five main-branch runs instead of the single latest one, so a lucky fast run on a hosted runner no longer makes every following PR look like a regression.

type: fixed

Packages ship one `THIRD-PARTY-NOTICES.md` again - the per-package file generated from each project's own NuGet closure - instead of failing to pack (`NU5118`) because the repository-wide file was still added alongside it.

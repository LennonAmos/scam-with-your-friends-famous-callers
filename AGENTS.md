Publish only this repository’s allowlisted product files. Never publish other output folders or their Git history.

Keep tests, fixtures, diagnostics, logs, screenshots, local configuration and real machine paths outside this repository. Use placeholders in examples.

Before every push or release, run Build.ps1 -CheckOnly and inspect tracked files, generated binaries and the embedded release archive for private data. Do not force-add ignored files.

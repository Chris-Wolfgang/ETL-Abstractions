// DocFX's "modern" template automatically loads `public/main.js` on every
// generated page — it is the one script file the template wires in by name.
// Plain static resources under `public/` (such as the canonical version
// picker, `version-picker.js`) are copied to the site but never referenced,
// so on their own they never run.
//
// This file is the wire-up: it loads the version picker as a classic script
// on every page. The picker itself is a framework-agnostic IIFE that ships
// unchanged from the canonical repo-template, so we deliberately do NOT inline
// it here — we just reference it. Resolving the URL via `import.meta.url`
// keeps the path correct regardless of the page's directory depth or the
// gh-pages repository prefix (e.g. `/ETL-Abstractions/public/`).
const picker = document.createElement('script');
picker.src = new URL('version-picker.js', import.meta.url).href;
picker.async = true;
document.head.appendChild(picker);

# Solution Common

`Solution.Common` is a shared code foundation originally developed for the *Solution* game project.
It provides a set of core utilities, patterns and abstractions designed to streamline Unity development.
While it powers *Solution*, it is fully usable as a standalone package in your own Unity projects.

## Installation

You can install `Solution.Common` via Unity's Package Manager using a Git URL.

**We recommend using the latest stable release tag** instead of `main` for maximum stability:

1. In Unity, open **Window** -> **Package Manager**.
2. Click **+** -> **Add package from git URL...**
3. Paste (replace `v1.0.0` with the desired release tag):

```
https://github.com/MrMystery10-del/Solution-Common.git#v1.0.0
```

4. You can find all available versions on the [Releases page](https://github.com/MrMystery10-del/Solution-Common/releases).

Unity will fetch the exact version you specify and make it available under the `Solution.Common` namespaces.

---

## Documentation

A full API reference is available in [Documentation](Documentation~/Common.md).

---

## Contributing

We welcome contributions that focus on **minor improvements** and **enhanced documentation**.
However, please note the following contribution guidelines:

* **New features or classes will not be accepted**, the core functionality is reserved for the *Solution* development team.
* All pull requests **must target the `development` branch**, PRs opened against `main` will be closed without review.
* Pull requests **must be well named and clearly described**:

  * PR titles should briefly state the change.
  * Descriptions should outline the reasoning and scope.
* Pull requests that do not follow these guidelines **will be ignored without review**.

If your contribution fits within these rules, feel free to open a pull request.
We appreciate any effort to make the codebase more maintainable and the documentation more helpful.

---

## Third-Party Notices
This package may include or depend on third-party software.
For details, see [Third Party Notices](/Third%20Party%20Notices.md).

---

## License

MIT License. See [LICENSE](LICENSE.md) for details.

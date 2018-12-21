# Contributing to Zeebe C# Client

## Building the C# client from source

The core functionality and API lies in the `Client` project sub folder.
Simply open the solution with one of your prefered IDE's (like VS Studio, Rider or Mono).
It should also be possible to build the solution via `msbuild`.

The `Client-test` project contains as the name states the tests of the Client API.

## Reporting issues or contact developers

This project uses GitHub issues to organize the development process. If you want to
report a bug or request a new feature feel free to open a new issue on
[GitHub][issues].

If you are reporting a bug, please help to speed up problem diagnosis by
providing as much information as possible. Ideally, that would include a small
[sample project][sample] that reproduces the problem.

If you have a general usage question please ask on the [forum][] or [slack][] channel.

## Work on an issue

Please refer the [Contributing](https://github.com/zeebe-io/zeebe/blob/master/CONTRIBUTING.md) details of zeebe project.

## GitHub Issue Guidelines

Please refer the [Contributing](https://github.com/zeebe-io/zeebe/blob/master/CONTRIBUTING.md) details of zeebe project.

## Commit Message Guidelines

The commit message should match the following pattern
```
%{type}(%{scope}): %{description}
```

- `type` and `scope` should be chosen as follows
    - `feat`: For user facing features or improvements. `scope` should be `client`
    - `fix`: For user facing bug fixes. `scope` should be either
      `client`, `test` or `example`.
    - `chore`: For code changes which are not user facing, `scope` should be either
      `client`, `test` or `example.
    - `docs`:  For changes on the documentation. `scope` should be either
      `client`, `test` or `example
- `description`: short description of the change to a max length of the whole
  subject line of 120 characters

## License

Most Zeebe source files are made available under the [Apache License, Version
2.0](/APACHE-2.0) except for the [broker-core](/broker-core) component. The
[broker-core](/broker-core) source files are made available under the terms of
the [GNU Affero General Public License (GNU AGPLv3)](/GNU-AGPL-3.0). See
individual source files for details.

If you would like to contribute something, or simply want to hack on the code
this document should help you get started.

## Code of Conduct

This project adheres to the Contributor Covenant [Code of
Conduct](/CODE_OF_CONDUCT.md). By participating, you are expected to uphold
this code. Please report unacceptable behavior to
code-of-conduct@zeebe.io.

[issues]: https://github.com/zeebe-io/zb-csharp-client/issues
[forum]: https://forum.zeebe.io/
[slack]: https://zeebe-slackin.herokuapp.com/
[sample]: https://github.com/zeebe-io/zb-csharp-client/tree/master/Client-Example

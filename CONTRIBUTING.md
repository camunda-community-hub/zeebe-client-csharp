# Contributing to Zeebe C# Client

## Building the C# client from source

The core functionality and API lies in the Client project.
As a prerequisite you need to generate as first the gRPC C# client.

If you have maven installed simply go into the `gateway-protocol/csharp-protocol` directory
and run maven. This will download the `gateway.proto` file from the Zeebe project.
You can also simply copy it by hand.

After that run either `gen-grpc.sh` or `win-gen-grpc.sh` dependening on the enviroment.
You can also take a look in the script and adjust if necessary.
This will generate the `Gateway.cs` and `GatewayGrpc.cs` files in the `Client/Impl/proto` directory.

It should be now possible to build the Client project. Simply open the solution with one of your prefered IDE's (like VS Studio, Rider or Mono).

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

The project follows a
[gitflow](https://nvie.com/posts/a-successful-git-branching-model/) workflow.
The `develop` branch contains the current in development state of the project,
where the `master` branch contains the latest stable release.

If you want to work on an issue please follow the following steps:

1. Check that a [GitHub issue][issues] exists for the task you want to work on,
   if not please create one first.
1. Checkout the `develop` branch and pull the latest changes.
   ```
   git checkout develop
   git pull
   ```
1. Create a new branch with the naming scheme `issueId-description`.
   ```
   git checkout -b 123-adding-bpel-support`
   ```
1. Implement the required changes on your branch and regularly push your
   changes to the origin so that the CI can run on it. Git commit will run a
   pre-commit hook which will check the formatting, style and license headers
   before committing. If these checks fail please fix the issues. Code format
   and license headers can be fixed automatically by running maven. Checkstyle
   violations have to be fixed manually.
   ```
   git commit -am 'feat(broker): bpel support'
   git push -u origin 123-adding-bpel-support
   ```
1. If you think you finished the issue please prepare the branch for reviewing.
   In general the commits should be squashed into meaningful commits with a
   helpful message. This means cleanup/fix etc commits should be squashed into
   the related commit. If you made refactorings or similar which are not
   directly necessary for the task it would be best if they are split up into
   another commit. Rule of thumb is that you should think about how a reviewer
   can best understand your changes. Please follow the [commit message
   guidelines](#commit-message-guidelines).
1. After finishing up the squashing force push your changes to your branch.
   ```
   git push --force-with-lease
   ```
1. To start the review process create a new pull request on GitHub from your
   branch to the `develop` branch. Give it a meaningful name and describe
   your changes in the body of the pull request. Lastly add a link to the issue
   this pull request closes, i.e. by writing in the description `closes #123`
1. Assign the pull request to one developer to review, if you are not sure who
   should review the issue skip this step. Someone will assign a reviewer for
   you.
1. The reviewer will look at the pull request in the following days and give
   you either feedback or accept the changes.
    1. If there are changes requested address them in a new commit. Notify the
       reviewer in a comment if the pull request is ready for review again. If
       the changes are accepted squash them again in the related commit and force push.
       Then initiate a merge by writing a comment with the contet `bors r+`.
    1. If no changes are requested the reviewer will initiate a merge by adding a
       comment with the content `bors r+`.
1. When a merge is initiated, a bot will merge your branch with the latest
   develop and run the CI on it.
    1. If everything goes well the branch is merged and deleted and the issue
       and pull request are cloesed.
    2. If there are merge conflicts the author of the pull request has to
       manually rebase `develop` into the issue branch and retrigger a merge
       attempt.
    3. If there are CI errors the author of the pull request has to check if
       they are caused by its changes and address them. If they are flaky tests
       a merge can be retried with a comment with the content `bors retry`.

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

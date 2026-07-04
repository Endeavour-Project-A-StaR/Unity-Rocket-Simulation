# Contributing

## Working agreement

Keep contributions small enough for another member to review. Each task should state the behaviour being changed, an owner, a reviewer, and how completion will be demonstrated. Pair on unfamiliar code and share a runnable result regularly.

Do not combine a physics change, scene reorganisation, and formatting cleanup in one pull request.

## Git workflow

Run commands from the repository root. Before starting:

```sh
git status
git fetch origin
git switch -c codex/short-task-name
```

Start from the team's agreed integration branch when the working tree is clean. If it contains someone else's work, preserve it and establish ownership before switching the base or staging files. Never reset or clean the checkout to make the status look tidy.

After making and checking a change:

```sh
git diff
git add path/to/changed-file
git diff --cached
git commit -m "docs: explain the reference simulation"
git push -u origin HEAD
```

Stage the specific intended files, including matching .meta files for changed assets. Open a pull request describing the change, its reason, and the checks performed. Use the repository's agreed integration branch as the target.

A detached HEAD can occur in an isolated checkout. Create a named task branch before committing or pushing. Do not force-push shared branches.

## Unity-specific rules

- Use the editor version in ProjectSettings/ProjectVersion.txt.
- Keep **Asset Serialization: Force Text** and **Version Control: Visible Meta Files**. Both are already configured.
- Move or rename assets through Unity so their GUIDs and .meta files remain paired.
- Coordinate scene editing: avoid two people rearranging RocketScene simultaneously.
- Review scene and asset diffs for accidental Inspector changes.
- Never resolve scene conflicts by blindly choosing one entire side. Ask the scene owner and verify the merged scene in Unity.
- .gitattributes marks Unity YAML as text for review; it does not install or configure UnityYAMLMerge.
- Do not commit Library, Temp, generated solutions, personal IDE settings, or ordinary flight logs.
- Do not run a repository-wide line-ending renormalisation as part of an unrelated change.

## Definition of done

- The changed behaviour has an appropriate check and its result is recorded.
- Physics changes state units, reference frames, assumptions, and expected behaviour.
- Documentation is updated where behaviour or installation changed.
- The example scene still opens and runs, or the PR clearly identifies why this could not be checked.
- Another member has reviewed the change.

Use the [verification guide](docs/verification.md). A working animation alone does not verify a force model.

## Results and decisions

Keep raw experimental data separate from generated runs. If deliberately adding a reference dataset, include its source, units, processing, and permission/licence information.

For a consequential decision, add a short document under docs/decisions containing context, options, decision, consequences, and evidence. Create these when decisions occur; no empty decision framework is required.

## Recommended GitHub settings

A repository administrator should confirm the integration branch, require pull requests with one review, and prevent force-pushes to that branch. Add required automated checks only after they actually exist and pass. These remote settings are recommendations, not configuration applied by this documentation change.
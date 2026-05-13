# Save Knowledge Command

Run this after completing any significant implementation. Creates a knowledge file that Claude can read in future sessions to understand exactly how something is built.

## When to run
- After implementing a complex feature (auth, repayment plan, email, CI/CD)
- After solving a tricky bug
- After making a non-obvious architectural decision in code

## Steps

### 1. Create knowledge file
Create `.claude/knowledge/[feature-name].md` with the following structure:

```markdown
# [Feature Name]

## Overview
Brief description of what this does and why it exists.

## Location
Which files are involved:
- `path/to/file.cs` — what it does
- `path/to/file.cs` — what it does

## How it works
Step by step explanation of the implementation.

## Key details
Specific things that are easy to forget:
- Why a specific library was chosen
- Non-obvious configuration
- Edge cases handled
- Things that MUST NOT be changed

## Code snippets
The most important parts of the implementation:
\```csharp
// key code here
\```

## Dependencies
- NuGet packages used and why
- External services configured

## How to extend
If someone needs to add something similar, here is how:
1. Step one
2. Step two
```

### 2. Reference in CLAUDE.md
If this knowledge file is critical, add it to the context files in root `CLAUDE.md`:
```markdown
- @.claude/knowledge/[feature-name].md — [brief description]
```

### 3. Commit
```bash
git add .claude/knowledge/[feature-name].md
git commit -m "docs: add knowledge file for [feature name]"
```
# Update Docs Command

Run this after every completed feature or significant change.

## Steps

### 1. Update PROGRESS.md
- Move completed item from `Backlog` to `Completed`
- Add date of completion
- Add any notes or issues encountered under `Notes & Decisions Made During Development`

### 2. Update DECISIONS.md (if needed)
- If any architectural decision was made during this feature, add it:
```markdown
### YYYY-MM-DD — Decision title
**Decision:** What was decided.
**Why:** Reasoning. What alternatives were considered and why rejected.
```

### 3. Update CONVENTIONS.md (if needed)
- If a new pattern was introduced that should be followed in future features, document it with a real code example

### 4. Commit the docs
```bash
git add PROGRESS.md DECISIONS.md CONVENTIONS.md
git commit -m "docs: update progress and decisions after [feature name]"
```
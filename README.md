# Extensive-Board-Game-Framework

IFN584 Assignment 3: a board game framework in C# / .NET 10 (Gomoku and Reversi, three variants each).

## Requirements

- .NET 10 SDK (`dotnet --list-sdks` should show `10.0.x`)

## Repository layout

```
BoardGames.sln
src/
├── BoardGames.Core/        Class library: all game logic
│   ├── Engine/             Stream 1
│   ├── Model/              Stream 1 (Pieces/)
│   ├── Players/            Stream 2 (Strategies/)
│   ├── Rules/              Streams 1 & 2 (Gomoku/, Reversi/)
│   ├── Commands/           Stream 3
│   ├── Persistence/        Stream 3
│   └── Variants/           Streams 1 & 2 (Gomoku/, Reversi/)
└── BoardGames.Cli/         Console app, Stream 4 (Parsing/); references Core
tests/
└── BoardGames.Tests/       xUnit; mirrors src/ folders; references Core
```

Namespaces follow folders (e.g. `src/BoardGames.Core/Rules/Reversi/` → `BoardGames.Core.Rules.Reversi`). Keep the code consistent with the class diagram in the report.

## Build, run, test

```bash
dotnet clean && dotnet build                         # must finish with 0 errors
dotnet run --project src/BoardGames.Cli             # interactive mode
dotnet test                                          # run the unit test suite
```

CLI test mode (from the spec, §4.5):

```bash
dotnet run --project src/BoardGames.Cli -- --game reversi --variant anti "P4:3,P3:3,P3:4,P5:3,P6:3"
```

## Git workflow

- `main` should always build and pass `dotnet test`.
- Work on a feature branch (`feature/<short-name>`), then open a pull request into `main`.
- Get at least one teammate to review before merging. Reviews also show who contributed what.
- Commit under your own name and email so the history matches the contribution claims.

## Submission checklist (Group_XX.zip)

- [ ] Full source code plus `Group_XX.cast`
- [ ] No `bin/`, `obj/`, `.vs/`, `.idea/` or `.DS_Store` (use `git archive` or zip a fresh clone)
- [ ] `dotnet clean && dotnet build` gives 0 errors on a fresh clone

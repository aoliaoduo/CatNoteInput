# Repository Guidelines

## Project Structure & Module Organization
- `App.xaml` and `MainWindow.xaml` define the WPF UI and styling resources.
- Core logic lives in:
  - `ViewModels/` (UI state and commands)
  - `Services/` (API client and settings persistence)
  - `Models/` (data contracts like `AppSettings`)
  - `Infrastructure/` (helpers like `RelayCommand`)
  - `Converters/` (UI converters)
- Assets are in `Assets/` (icon).
- Build artifacts are excluded via `.gitignore` (`bin/`, `obj/`).

## Build, Test, and Development Commands
- `dotnet build` — builds the WPF app locally.
- `dotnet run` — runs the app from source.
- `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` — produces the single-file `exe` in `bin/Release/net8.0-windows/win-x64/publish/`.
- CI runs `dotnet restore` and `dotnet build --configuration Release`.

## Coding Style & Naming Conventions
- Follow `.editorconfig` (4-space indentation for `.cs`, `.xaml`, `.csproj`).
- Use PascalCase for classes and public members; camelCase for locals/fields.
- Keep UI strings in XAML or ViewModel constants; prefer short, clear Chinese labels.
- Avoid adding heavy dependencies unless UI or build clearly needs them.

## Testing Guidelines
- No automated tests are currently included.
- If adding tests, document the framework and add a `dotnet test` section here.
- Name tests descriptively (e.g., `SettingsService_Saves_WhenRememberSecretTrue`).

## Commit & Pull Request Guidelines
- Commit messages are short, imperative, and descriptive (e.g., “Add release workflow and metadata”).
- PRs should include: a short summary, screenshots for UI changes, and linked issues if applicable.
- Ensure CI passes before requesting review.

## Security & Configuration Tips
- Local settings are stored at `%AppData%\CatNoteInput\settings.json`.
- The API secret is stored as plain text (by design); do not commit secrets.
- Release packaging is triggered by pushing a tag like `v0.1.0`.

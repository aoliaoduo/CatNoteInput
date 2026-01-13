# CatNoteInput

A minimal Windows WPF client for sending notes to CatNote via the official API.

## Features
- Send plain-text notes (up to 5000 characters)
- Accepts either your `yoursecret` value or the full API URL
- Saves the API secret locally (optional)
- Friendly status feedback and shortcuts (Ctrl+Enter)

## Requirements
- Windows 10/11
- .NET 8 SDK (for building)

## Build
```bash
dotnet build
```

## Publish (single-file EXE)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The output EXE is in:
`bin/Release/net8.0-windows/win-x64/publish/`

## Configuration
Settings are stored at:
`%AppData%\CatNoteInput\settings.json`

## API
The app sends requests to:
`https://api.catnote.cn/sapi/{yoursecret}`

## License
MIT (see `LICENSE`).

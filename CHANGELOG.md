# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-07-31

### Added
- Added `EnableDebugLogs` toggle to allow turning off SDK debug messages in production.
- Added strongly-typed response models (`AuthResponse`, `LeaderboardEntry`, `PlayerDataResponse`) for easier data handling.
- Added request timeout handling to `UnityWebRequest` (15 seconds default).
- Added `CHANGELOG.md` for Unity Package Manager specification compliance.

### Changed
- Refactored `AstenSDK` singleton with `[AddComponentMenu]` and `[DisallowMultipleComponent]`.
- Enhanced security by masking secret API keys in console initialization logs.
- Improved JSON parsing robustness for authentication tokens and player IDs.

### Fixed
- Fixed raw API key print vulnerability in `AstenSDK.Initialize`.
- Cleaned up OS system files (`.DS_Store`) from repository folders.

## [1.0.0] - 2026-01-15

### Added
- Initial release of Asten BaaS Unity SDK.
- Device ID (Guest) and Email OTP Authentication.
- Cloud Data Persistence (Cloud Saves) with built-in 3-second debounce cooldown.
- Global Leaderboards support (submit scores & fetch top rankings).
- WebGL, Android, iOS, and PC multi-platform support.
- Quickstart `OnGUI` interactive sample scene.

# Changelog

## [Unreleased]

### Added
- ResultsView filter section redesigned as a right-side panel.
- Introduced reusable, searchable, scrollable, multi-select controls for log level, source, and log filters.
- Multi-select controls support cross-filtering, dynamic counts, virtualization, keyboard/mouse accessibility, and theming.
- Placeholder for future time filter UI in the filter panel.
- LiveScanView now features a real-time, per-source scan progress UI with status icons, event counts, and progress bars.
- Scan statistics (sources scanned, completed, errors, in progress, total events) are updated live and efficiently, even with many sources.
- Performance improvements: atomic counters and thread-safe updates ensure responsiveness for large event log sets.
- Status icon and color mapping is now consistent between LiveScanView and ResultsView.

### Changed
- ResultsViewModel updated to support cross-filtering and dynamic counts for all filter controls.


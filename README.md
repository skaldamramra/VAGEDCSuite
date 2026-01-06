# VAG EDC Suite

A .NET-based tuning suite for Volkswagen Audi Group (VAG) Engine Control Units. This tool enables discovery, visualization, and modification of engine management maps in mostly Bosch EDC15 series ECUs.

## Features

- **Map Detection & Editing**: Automated detection of fuel, turbo, and limiter maps with 2D and 3D visualization.
- **Supported ECUs**: 
  - EDC15 (P, V, M, C)
- **Advanced Tools**:
  - Integrated Hex Viewer
  - Checksum verification and correction
  - VIN Decoding and Part Number lookup
  - EEProm editor and Bin Merger
  - XDF export for TunerPro compatibility
- **Modern Interface**: Migrated to the Krypton Toolkit for a streamlined, dark-themed user experience.

## Technical Overview

- **Framework**: .NET 4.0
- **UI**: Windows Forms with Krypton Ribbon and Docking
- **Graphics**: OpenTK for 3D map rendering, ZedGraph for 2D charts
- **Architecture**: Moving towards a declarative, rule-based map detection engine (XML-driven) to separate domain logic from the core parser code.

## Development

### Prerequisites
- Visual Studio 2008 or newer
- .NET Framework 4.0
- Krypton Toolkit libraries (included in `lib/Krypton`)

### Build
Open `VAGSuite.sln` in Visual Studio and build the solution. Dependencies are managed via NuGet and local references in the `lib` folder.

## License
This project is freeware. See the `LICENSE` file for details.
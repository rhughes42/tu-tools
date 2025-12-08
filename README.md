# TU Tools

Fabrication tools for Technical University Dublin by Axis Consulting.

## Overview

TU Tools is a Grasshopper plugin that provides CNC machining capabilities for Rhino 6. It allows users to generate, compile, and export G-code toolpaths directly from Grasshopper definitions. The plugin includes authentication features and is designed specifically for educational use at Technical University Dublin.

## Features

- **CNC Toolpath Generation**: Convert points into G-code cutting commands
- **Program Compilation**: Compile individual operations into complete CNC programs
- **File Export**: Export programs to Axiom machine file format (.mmg)
- **Authentication**: Secure login system using Auth0 with 3-day token validity
- **Utility Functions**: Helper methods for geometry operations and plane interpolation

## Components

### Core
- **Login**: Authenticate users with DIT email addresses to access CNC features

### CNC
- **Cut**: Generate G-code cutting commands from a list of points
  - Input: List of points, feed speed (mm/s)
  - Output: G-code commands
  
- **File**: Compile individual CNC operations into a complete program
  - Input: List of cut commands
  - Output: Formatted CNC program with headers and footers
  
- **Export**: Export compiled programs to machine files
  - Input: Program commands, filename, filepath, export trigger
  - Output: .mmg file with export log

## Installation

### Prerequisites
- Rhino 6 or later
- Grasshopper (included with Rhino 6+)
- .NET Framework 4.6.1 or later

### Build from Source

1. Clone this repository:
   ```bash
   git clone https://github.com/rhughes42/tu-tools.git
   ```

2. Open `TUTools.sln` in Visual Studio 2015 or later

3. Restore NuGet packages:
   - Right-click solution in Solution Explorer
   - Select "Restore NuGet Packages"

4. Build the solution:
   - For development: Build in Debug mode (outputs to Grasshopper Libraries folder)
   - For distribution: Build in Release mode (outputs to bin folder)

5. The post-build event will automatically:
   - Copy the DLL to the appropriate location
   - Rename it to `.gha` extension
   - Clean up unnecessary files

### Installation

1. Copy `TUTools.gha` to your Grasshopper libraries folder:
   - Windows: `%AppData%\Grasshopper\Libraries\`

2. Unblock the file:
   - Right-click the .gha file
   - Select Properties
   - Click "Unblock" if present
   - Click OK

3. Restart Rhino and Grasshopper

## Usage

### Basic Workflow

1. **Login**: 
   - Place a Login component in your Grasshopper definition
   - Connect a boolean toggle to the Run input
   - Set to True to authenticate with your DIT email
   - Authentication is valid for 3 days

2. **Create Toolpath**:
   - Generate or import a list of 3D points representing your toolpath
   - Connect points to the Cut component
   - Optionally specify feed speed (default: 40 mm/s)

3. **Compile Program**:
   - Connect Cut component output to the File component
   - The program will include necessary initialization and cleanup commands

4. **Export**:
   - Connect File output to Export component
   - Specify filename and output path
   - Set Export boolean to True to write file

### Example

```
Points → Cut → File → Export → .mmg file
         ↓
      Speed (optional)
```

## Code Structure

```
TUTools/
├── CNC/
│   ├── Cut.cs          # Toolpath generation
│   ├── Program.cs      # Program compilation
│   └── Export.cs       # File export
├── Core/
│   └── Login.cs        # Authentication
├── Properties/
│   ├── AssemblyInfo.cs
│   └── Settings.Designer.cs
├── Resources/          # Icons and assets
├── Utilities.cs        # Helper functions
└── TUToolsInfo.cs     # Plugin metadata
```

## Development

### Code Style
- Use XML documentation comments for all public methods and classes
- Follow C# naming conventions (PascalCase for public members)
- Use constants for magic numbers
- Include input validation and error handling

### Building
- Debug builds output to: `%AppData%\Roaming\Grasshopper\Libraries\TU Tools\`
- Release builds output to: `bin\`

### Dependencies
- RhinoCommon (Rhino 6)
- Grasshopper SDK
- Auth0.OidcClient.WPF (2.4.3)
- Newtonsoft.Json (11.0.1)

## G-Code Reference

The generated G-code follows standard CNC conventions:

- **G00**: Rapid positioning
- **G01**: Linear interpolation (cutting)
- **G21**: Metric units (mm)
- **G90**: Absolute positioning mode
- **M03**: Spindle on (clockwise)
- **M09**: Coolant off
- **M30**: Program end

Default machine settings:
- Spindle speed: 12000 RPM
- Safe height: 50mm
- Feed rate: 2400 mm/min (for rapid moves)

## License

Copyright Axis © 2019

## Contact

- **Developer**: Axis Consulting
- **Email**: rhu@axisarch.tech
- **Organization**: Technical University Dublin

## Contributing

This is an educational project for Technical University Dublin. For questions or issues, please contact the development team at rhu@axisarch.tech.

## Version History

- **v1.0.0** (2019): Initial release
  - Core CNC functionality
  - Auth0 authentication
  - Axiom file export

## Acknowledgments

- Quaternion interpolation code adapted from the [Robots plugin](https://github.com/visose/Robots) by Vicente Soler
- Built for Technical University Dublin
- Developed by Axis Consulting

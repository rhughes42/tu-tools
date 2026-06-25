# Axis CNC (Grasshopper)

This plugin now ships additional CNC helpers focused on fabrication setup:

- **Calibration Offsets** – translate/rotate/scale toolpaths to reflect machine calibration.
- **Layout Planner** – create grid, mirror, and rotation layouts for multi-part jobs.
- **Tool Configuration** – bundle tool number, diameter, feeds, coolant and emit header lines.
- **Custom Command** – token-based templating for bespoke G-code lines.
- **Cut Time (Physics)** – estimate time using travel distance, acceleration, mass/inertia, and rapid classification.

All components sit in the `CNC` tab inside the **Axis CNC** category. The physics estimator uses a trapezoidal profile and reduces acceleration based on provided mass/inertia to approximate real machine behavior.

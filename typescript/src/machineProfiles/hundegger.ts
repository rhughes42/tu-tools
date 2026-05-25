import type { MachineProfile } from "./types";

export const hundeggerProfile: MachineProfile = {
  name: "hundegger",
  transformCommands(commands: string[]) {
    return [
      "(HUNDEGGER PROFILE)",
      ...commands.map((cmd) => cmd.replace("G0 ", "G00 ").replace("G1 ", "G01 ")),
    ];
  },
  validate(commands: string[]) {
    const issues: string[] = [];
    if (!commands.some((x) => x.toUpperCase().includes("M03"))) {
      issues.push("Hundegger profile expects spindle start command (M03).");
    }
    if (!commands.some((x) => x.toUpperCase().includes("M06") && x.toUpperCase().includes("T"))) {
      issues.push("Hundegger profile expects tool change command (Tn M06).");
    }
    return issues;
  },
};

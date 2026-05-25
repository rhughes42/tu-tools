import { hundeggerProfile } from "./hundegger";
import type { MachineProfile } from "./types";

const genericProfile: MachineProfile = {
  name: "generic",
  transformCommands(commands: string[]) {
    return commands;
  },
  validate() {
    return [];
  },
};

export function resolveMachineProfile(name?: string): MachineProfile {
  if ((name ?? "").toLowerCase() === "hundegger") {
    return hundeggerProfile;
  }
  return genericProfile;
}

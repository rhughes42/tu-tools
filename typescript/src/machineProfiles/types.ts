export interface MachineProfile {
  name: string;
  transformCommands(commands: string[]): string[];
  validate(commands: string[]): string[];
}

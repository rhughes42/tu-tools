export declare function normalizeCommands(commands: string[]): string[];
export declare function addLineNumbers(commands: string[], start?: number, step?: number): string[];
export declare function renderCustomCommandSafe(template: string, parameters: Record<string, string | number>): string;
export declare function toolHeader(cfg: {
    toolNumber: number;
    diameter: number;
    spindleRpm: number;
    feedRate: number;
    plungeRate: number;
    material: string;
    coolant: boolean;
}): string[];

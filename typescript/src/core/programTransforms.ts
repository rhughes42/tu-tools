export function normalizeCommands(commands: string[]): string[] {
  return [...new Set(commands.map((x) => x.trim()).filter((x) => x.length > 0))];
}

export function addLineNumbers(commands: string[], start = 10, step = 10): string[] {
  let line = start;
  return commands.map((cmd) => {
    const result = `N${line} ${cmd}`;
    line += step;
    return result;
  });
}

export function renderCustomCommandSafe(template: string, parameters: Record<string, string | number>): string {
  if (!template.trim()) return "";

  let result = Object.keys(parameters).reduce((cmd, key) => {
    return cmd.replace(new RegExp(`\\{${key}\\}`, "g"), String(parameters[key]));
  }, template);

  // Remove unresolved placeholders.
  result = result.replace(/\{[^{}]+\}/g, "");
  return result.trim();
}

export function toolHeader(cfg: {
  toolNumber: number;
  diameter: number;
  spindleRpm: number;
  feedRate: number;
  plungeRate: number;
  material: string;
  coolant: boolean;
}): string[] {
  const lines = [
    `( Tool ${cfg.toolNumber} | Dia ${cfg.diameter.toFixed(2)}mm | ${cfg.material} )`,
    `T${cfg.toolNumber} M06`,
    `S${cfg.spindleRpm.toFixed(0)} M03`,
    `F${cfg.feedRate.toFixed(1)}`,
    `( Plunge ${cfg.plungeRate.toFixed(1)} mm/min )`,
  ];

  if (cfg.coolant) lines.push("M08");
  return lines;
}

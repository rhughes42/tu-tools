export type TelemetryContext = {
  environment: string;
  release: string;
  module: string;
};

export type TelemetryEvent = {
  level: "debug" | "info" | "warn" | "error";
  message: string;
  context: TelemetryContext;
  timestamp: string;
  tags?: Record<string, string>;
  data?: Record<string, string>;
};

export interface TelemetrySink {
  emit(event: TelemetryEvent): Promise<void>;
}

export class ConsoleTelemetrySink implements TelemetrySink {
  async emit(event: TelemetryEvent): Promise<void> {
    // eslint-disable-next-line no-console
    console.log(`[${event.timestamp}] ${event.level} ${event.context.module}: ${event.message}`);
  }
}

export class SentryCompatibleTelemetrySink implements TelemetrySink {
  constructor(private readonly endpoint?: string) {}

  async emit(event: TelemetryEvent): Promise<void> {
    if (!this.endpoint) return;

    try {
      await fetch(this.endpoint, {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({
          message: event.message,
          level: event.level,
          environment: event.context.environment,
          release: event.context.release,
          module: event.context.module,
          timestamp: event.timestamp,
          tags: event.tags,
          extra: event.data,
        }),
      });
    } catch {
      // Non-blocking telemetry path.
    }
  }
}

export class TelemetryClient {
  constructor(private readonly sampleRate: number, private readonly sinks: TelemetrySink[]) {}

  async emit(
    level: TelemetryEvent["level"],
    message: string,
    context: TelemetryContext,
    tags?: Record<string, string>,
    data?: Record<string, string>
  ): Promise<void> {
    if (!this.sinks.length) return;
    const normalizedSampleRate = Math.max(0, Math.min(1, this.sampleRate));
    if (Math.random() > normalizedSampleRate) return;

    const event: TelemetryEvent = {
      level,
      message: redact(message),
      context,
      timestamp: new Date().toISOString(),
      tags,
      data: Object.fromEntries(Object.entries(data ?? {}).map(([k, v]) => [k, redact(v)])),
    };

    for (const sink of this.sinks) {
      await sink.emit(event);
    }
  }
}

function redact(input: string): string {
  return input
    .replace(/[A-Za-z]:\\[^\s]+/g, "[path-redacted]")
    .replace(/(token|apikey|secret)=([^\s]+)/gi, "$1=[redacted]");
}

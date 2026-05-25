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
export declare class ConsoleTelemetrySink implements TelemetrySink {
    emit(event: TelemetryEvent): Promise<void>;
}
export declare class SentryCompatibleTelemetrySink implements TelemetrySink {
    private readonly endpoint?;
    constructor(endpoint?: string | undefined);
    emit(event: TelemetryEvent): Promise<void>;
}
export declare class TelemetryClient {
    private readonly sampleRate;
    private readonly sinks;
    constructor(sampleRate: number, sinks: TelemetrySink[]);
    emit(level: TelemetryEvent["level"], message: string, context: TelemetryContext, tags?: Record<string, string>, data?: Record<string, string>): Promise<void>;
}

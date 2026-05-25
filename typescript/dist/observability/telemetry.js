"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TelemetryClient = exports.SentryCompatibleTelemetrySink = exports.ConsoleTelemetrySink = void 0;
class ConsoleTelemetrySink {
    async emit(event) {
        // eslint-disable-next-line no-console
        console.log(`[${event.timestamp}] ${event.level} ${event.context.module}: ${event.message}`);
    }
}
exports.ConsoleTelemetrySink = ConsoleTelemetrySink;
class SentryCompatibleTelemetrySink {
    constructor(endpoint) {
        this.endpoint = endpoint;
    }
    async emit(event) {
        if (!this.endpoint)
            return;
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
        }
        catch {
            // Non-blocking telemetry path.
        }
    }
}
exports.SentryCompatibleTelemetrySink = SentryCompatibleTelemetrySink;
class TelemetryClient {
    constructor(sampleRate, sinks) {
        this.sampleRate = sampleRate;
        this.sinks = sinks;
    }
    async emit(level, message, context, tags, data) {
        if (!this.sinks.length)
            return;
        const normalizedSampleRate = Math.max(0, Math.min(1, this.sampleRate));
        if (Math.random() > normalizedSampleRate)
            return;
        const event = {
            level,
            message: redact(message),
            context,
            timestamp: new Date().toISOString(),
            tags,
            data: Object.fromEntries(Object.entries(data !== null && data !== void 0 ? data : {}).map(([k, v]) => [k, redact(v)])),
        };
        for (const sink of this.sinks) {
            await sink.emit(event);
        }
    }
}
exports.TelemetryClient = TelemetryClient;
function redact(input) {
    return input
        .replace(/[A-Za-z]:\\[^\s]+/g, "[path-redacted]")
        .replace(/(token|apikey|secret)=([^\s]+)/gi, "$1=[redacted]");
}

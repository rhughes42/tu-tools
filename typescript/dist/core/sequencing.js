"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.sequenceNearestNeighbor = sequenceNearestNeighbor;
function sequenceNearestNeighbor(points) {
    if (points.length <= 2)
        return points;
    const remaining = [...points];
    const ordered = [remaining.shift()];
    while (remaining.length) {
        const last = ordered[ordered.length - 1];
        let nearestIndex = 0;
        let nearestDistance = Number.POSITIVE_INFINITY;
        for (let i = 0; i < remaining.length; i++) {
            const d = distanceTo(last, remaining[i]);
            if (d < nearestDistance) {
                nearestDistance = d;
                nearestIndex = i;
            }
        }
        ordered.push(remaining.splice(nearestIndex, 1)[0]);
    }
    return ordered;
}
function distanceTo(a, b) {
    const dx = a.x - b.x;
    const dy = a.y - b.y;
    const dz = a.z - b.z;
    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}

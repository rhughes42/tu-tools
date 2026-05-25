"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.resolveMachineProfile = resolveMachineProfile;
const hundegger_1 = require("./hundegger");
const genericProfile = {
    name: "generic",
    transformCommands(commands) {
        return commands;
    },
    validate() {
        return [];
    },
};
function resolveMachineProfile(name) {
    if ((name !== null && name !== void 0 ? name : "").toLowerCase() === "hundegger") {
        return hundegger_1.hundeggerProfile;
    }
    return genericProfile;
}

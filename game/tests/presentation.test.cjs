'use strict';

const assert = require('node:assert/strict');
const presentation = require('../presentation.js');

const mixes = [0, 1, 2, 3].map(presentation.audioLevels);
for (let i = 1; i < mixes.length; i++) {
  assert.ok(mixes[i].noise < mixes[i - 1].noise, 'each generator lowers broadband noise');
  assert.ok(mixes[i].drone < mixes[i - 1].drone, 'each generator lowers the low-frequency drone');
}
assert.ok(mixes[3].noise > 0 && mixes[3].drone > 0, 'repaired power leaves a faint room tone');

const fitted = presentation.fitToHeight({ x: 18, y: 24, width: 90, height: 180 }, 90);
assert.deepEqual(fitted, { x: 18, y: 24, width: 45, height: 90 }, 'alpha-trimmed sprites fit the same visible height without distortion');
assert.equal(presentation.fitToHeight(null, 90), null, 'missing alpha bounds safely fall back');

for (let i = 0; i <= 100; i++) {
  const alpha = presentation.emergencyPulse(i / 10);
  assert.ok(alpha >= 0 && alpha <= 0.350001, 'emergency pulse stays within the requested red-light opacity range');
}
const alarmPeriods = [0, 1, 2, 3].map(presentation.alarmPeriod);
for (let i = 1; i < alarmPeriods.length; i++) {
  assert.ok(alarmPeriods[i] < alarmPeriods[i - 1], 'each generator shortens the emergency alarm cycle');
}
assert.ok(alarmPeriods.every(period => period >= 0.65 && period <= 3), 'alarm pacing remains within readable limits');
assert.ok(Math.abs(presentation.interactionBob(0)) < 0.001, 'interaction hint begins at its resting position');
assert.ok(presentation.interactionBob(0.35) > 0 && presentation.interactionBob(1.05) < 0, 'interaction hint floats above and below its resting position');
for (let i = 0; i < 100; i++) assert.ok(Math.abs(presentation.interactionBob(i * 0.05)) <= 2, 'interaction hint stays within two pixels');

console.log('presentation rules passed');

'use strict';

const assert = require('node:assert/strict');
const bossAI = require('../boss-ai.js');

const boss = { kind: 'nexus', x: 650, phase: 2, counterOpen: 0, moveSide: 1 };
const first = bossAI.move(boss, { x: 300 }, 0.5, 960);
assert.ok(first < 650, 'NEXUS closes distance instead of remaining fixed in the arena');
boss.counterOpen = 0.5;
assert.equal(bossAI.move(boss, { x: 100 }, 0.5, 960), first, 'NEXUS stays punishable during its recovery');
boss.counterOpen = 0;
for (let i = 0; i < 80; i++) bossAI.move(boss, { x: i % 2 ? 0 : 960 }, 0.5, 960);
assert.ok(boss.x >= 135 && boss.x <= 825, 'boss movement respects arena margins');

const patternBoss = { phase: 3 };
const firstCycle = Array.from({ length: 4 }, () => bossAI.nextPattern(patternBoss, 4, () => 0.4));
assert.deepEqual(firstCycle, [0, 1, 2, 3], 'every final-phase move appears before the sequence reshuffles');
let previous = firstCycle.at(-1);
for (let i = 0; i < 12; i++) {
  const next = bossAI.nextPattern(patternBoss, 4, () => 0.4);
  assert.notEqual(next, previous, 'the same boss move never repeats immediately');
  previous = next;
}

assert.ok(Math.abs(bossAI.rushTarget({ x: 650 }, { x: 300 }, 960) - 650) >= 200, 'rush traverses meaningful ground');
console.log('boss AI rules passed');

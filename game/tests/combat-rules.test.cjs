'use strict';

const assert = require('node:assert/strict');
const combat = require('../combat.js');

assert.deepEqual(combat.attackProfile(0), {
  charged: false,
  startup: 0.1,
  active: 0.1,
  recovery: 0.28,
  cooldown: 0.48,
  damageMultiplier: 1,
  moveMultiplier: 1,
});

assert.deepEqual(combat.attackProfile(0.65), {
  charged: true,
  startup: 0.18,
  active: 0.14,
  recovery: 0.42,
  cooldown: 0.74,
  damageMultiplier: 2.4,
  moveMultiplier: 0.6,
});
assert.deepEqual(combat.attackProfile(1.4), combat.attackProfile(0.65));
assert.equal(combat.chargeMoveMultiplier(0.01), 0.6);
assert.equal(combat.chargeMoveMultiplier(0), 1);
assert.equal(combat.DASH_COOLDOWN_SECONDS, 5);
assert.deepEqual(combat.playerPhaseProfile(1), { speed: 150, damageTakenMultiplier: 0.45 });
assert.deepEqual(combat.playerPhaseProfile(2), { speed: 210, damageTakenMultiplier: 0.75 });
assert.deepEqual(combat.playerPhaseProfile(3), { speed: 280, damageTakenMultiplier: 1.1 });

const player = { hp: 55, max: 100, potions: 2 };
assert.equal(combat.usePotion(player), 10);
assert.deepEqual(player, { hp: 65, max: 100, potions: 1 });
assert.equal(combat.usePotion({ hp: 100, max: 100, potions: 2 }), 0);
assert.equal(combat.usePotion({ hp: 50, max: 100, potions: 0 }), 0);
assert.equal(combat.usePotion({ hp: 7, max: 100, potions: 1, alive: false }), 0);
assert.equal(combat.hazardHitsPlayer({ kind: 'rush' }, { x: 205, y: 448 }, { x: 200, w: 118, h: 168 }, 448, 960), true, 'moving boss body can hit a nearby player');
assert.equal(combat.hazardHitsPlayer({ kind: 'rush' }, { x: 410, y: 448 }, { x: 200, w: 118, h: 168 }, 448, 960), false, 'rush collision follows the boss rather than covering the whole lane');

console.log('PASS: combat rule checks');

'use strict';
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const appearance = require('../player-appearance.js');

assert.equal(appearance.selectPlayerVisual(1, {}).name, 'Player_P1');
assert.equal(appearance.selectPlayerVisual(2, { charging: true }).name, 'Player_Evolution_P2');
assert.equal(appearance.selectPlayerVisual(3, { charging: true }).name, 'Player_Evolution_P3');
assert.equal(appearance.selectPlayerVisual(2, { charging: true }).frame, 4, 'phase two charges in its whole-body brace pose');
assert.equal(appearance.selectPlayerVisual(3, { charging: true }).frame, 4, 'phase three charges in its whole-body brace pose');
assert.equal(appearance.selectPlayerVisual(2, { attack: true, attackProgress: 0.35 }).frame, 5, 'phase two uses a straight strike pose');
assert.equal(appearance.selectPlayerVisual(3, { attack: true, attackProgress: 0.35 }).frame, 5, 'phase three uses a straight strike pose');
assert.equal(appearance.selectPlayerVisual(3, { hurt: true }).frame, 7, 'human form gets a whole-body hit reaction');
assert.equal(appearance.selectPlayerVisual(3, { moving: true, elapsed: 0.1 }).frame, 2, 'human form has dedicated movement poses');
assert.equal(appearance.selectPlayerVisual(1, { suitDamageLevel: 2 }).name, 'Player_Evolution_P3', 'repairing health does not restore the armor silhouette');

const manifestText = fs.readFileSync(path.join(__dirname, '..', 'asset-manifest.js'), 'utf8');
const registry = JSON.parse(manifestText.slice('window.LAST_SPARK_SPRITES = '.length, manifestText.lastIndexOf(';')));
for (const phase of [2, 3]) {
  const key = `Player_Evolution_P${phase}`;
  const asset = registry[key];
  assert.equal(asset.frames, 9, `${key} contains all full-body poses`);
  const png = fs.readFileSync(path.join(__dirname, '..', asset.src));
  assert.equal(png.readUInt32BE(16), 768, `${key} atlas width`);
  assert.equal(png.readUInt32BE(20), 768, `${key} atlas height`);
  assert.equal(png[25], 6, `${key} retains transparent RGBA pixels`);
}

const runtimeContext = {
  LAST_SPARK_SPRITES: registry,
  Image: class { constructor() { this.complete = true; this.naturalWidth = 768; } set src(value) { this.path = value; } }
};
runtimeContext.window = runtimeContext;
vm.runInNewContext(fs.readFileSync(path.join(__dirname, '..', 'asset-runtime.js'), 'utf8'), runtimeContext);
const sourceRects = [];
const canvas = { globalAlpha: 1, save() {}, restore() {}, translate() {}, scale() {}, drawImage(...args) { sourceRects.push(args.slice(1, 5)); } };
assert.equal(runtimeContext.LastSparkAssets.draw(canvas, 'Player_Evolution_P2', 0, 16, 16, { frame: 5, baseScale: 0.4 }), true);
assert.deepEqual(sourceRects[0], [512, 256, 256, 256], 'explicit frame selection draws the straight-punch cell');

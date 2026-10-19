'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const calls = { gradients: 0, arcs: 0, fills: 0, rectangles: 0, images: 0, maskCutouts: 0 };
const context = {
  save() {}, restore() {}, fillRect() { calls.rectangles++; }, beginPath() {}, moveTo() {}, closePath() {}, drawImage() { calls.images++; },
  arc() { calls.arcs++; }, fill() { calls.fills++; },
  createRadialGradient() { calls.gradients++; return { addColorStop() {} }; }
};
const maskContext = {
  save() {}, restore() {}, clearRect() {}, fillRect() {}, beginPath() {}, moveTo() {}, closePath() {}, arc() {}, fill() {},
  createRadialGradient() { return { addColorStop() {} }; }
};
Object.defineProperty(maskContext, 'globalCompositeOperation', { set(value) { if (value === 'destination-out') calls.maskCutouts++; } });
const maskCanvas = { width: 0, height: 0, getContext() { return maskContext; } };
const sandbox = { window: {}, document: { createElement() { return maskCanvas; } } };
vm.runInNewContext(fs.readFileSync(path.join(__dirname, '..', 'lighting.js'), 'utf8'), sandbox);
sandbox.window.LastSparkLighting.draw(context, 960, 540, 0.7,
  [{ x: 120, y: 180, radius: 110, intensity: 0.25 }],
  { x: 240, y: 300, face: -1, radius: 280 }, 0.4);

assert.ok(calls.gradients >= 5, 'draw creates room, flashlight, and emergency beacon gradients');
assert.ok(calls.arcs >= 4, 'draw keeps point lights and adds a directional flashlight cone');
assert.ok(calls.fills >= 4 && calls.rectangles >= 3, 'draw composes the light and alarm layers');
assert.ok(calls.maskCutouts > 0 && calls.images > 0, 'flashlight cuts a transparent window in a separate darkness layer so the map remains visible inside the beam');
assert.ok(sandbox.window.LastSparkLighting.sparkIntensity(3) > 0, 'damaged wires can still spark after all generators are restored');
assert.deepEqual([0, 1, 2, 3].map(sandbox.window.LastSparkLighting.alpha), [1, 0.6, 0.32, 0.02], 'power progressively clears the darkness and fully restores room lighting');
assert.deepEqual([0, 1, 2, 3].map(sandbox.window.LastSparkLighting.flashlightRadius), [145, 210, 280, 145], 'flashlight has a narrow emergency cone at startup and a manual fallback after restoration');

console.log('lighting render rules passed');

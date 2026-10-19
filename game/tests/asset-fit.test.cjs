'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const drawCalls = [];
const pixels = new Uint8ClampedArray(32 * 32 * 4);
for (let y = 8; y < 28; y++) {
  for (let x = 4; x < 12; x++) pixels[(y * 32 + x) * 4 + 3] = 255;
}
const canvas = {
  width: 0,
  height: 0,
  getContext() {
    return {
      clearRect() {},
      drawImage() {},
      getImageData() { return { data: pixels }; }
    };
  }
};
const context = {
  LAST_SPARK_SPRITES: { TestPose: { src: 'pose.png', frames: 1, width: 32, height: 32, columns: 1, rows: 1, fps: 8, loop: false, pivot: [0.5, 0.125], scale: 1 } },
  Image: class { constructor() { this.complete = true; this.naturalWidth = 32; } set src(value) { this.path = value; } },
  document: { createElement: () => canvas },
  LastSparkPresentation: { fitToHeight: (bounds, height) => ({ x: bounds.x, y: bounds.y, width: bounds.width * height / bounds.height, height }) }
};
context.window = context;
vm.runInNewContext(fs.readFileSync(path.join(__dirname, '..', 'asset-runtime.js'), 'utf8'), context);
const drawContext = {
  globalAlpha: 1,
  save() {}, restore() {}, translate() {}, scale() {},
  drawImage(...args) { drawCalls.push(args); }
};

assert.equal(context.LastSparkAssets.draw(drawContext, 'TestPose', 0, 100, 200, { fitHeight: 90 }), true);
assert.deepEqual(drawCalls[0].slice(1, 5), [4, 8, 8, 20], 'renderer crops transparent padding around the visible pose');
assert.deepEqual(drawCalls[0].slice(5), [82, 110, 36, 90], 'renderer holds visible sprite height and foot baseline steady');

pixels.fill(0);
context.LAST_SPARK_SPRITES.TestPoseFallback = context.LAST_SPARK_SPRITES.TestPose;
assert.equal(context.LastSparkAssets.draw(drawContext, 'TestPoseFallback', 0, 100, 200, { fitHeight: 90 }), true);
assert.deepEqual(drawCalls[1].slice(1, 5), [0, 0, 32, 32], 'inaccessible alpha data falls back to the stable full cell');
assert.deepEqual(drawCalls[1].slice(5), [55, 110, 90, 90], 'fallback still keeps its requested display height');

console.log('alpha-trimmed sprite fit passed');

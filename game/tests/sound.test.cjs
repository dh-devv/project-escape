'use strict';

const assert = require('node:assert/strict');
const sound = require('../sound.js');

assert.equal(sound.pitchMultiplier(() => 0, 0.06), 0.94, 'pitch spread has a stable lower bound');
assert.equal(sound.pitchMultiplier(() => 1, 0.06), 1.06, 'pitch spread has a stable upper bound');
assert.equal(sound.pitchMultiplier(() => 0.5, 0.06), 1, 'centered random value preserves the authored pitch');
assert.ok(sound.cue('alarm').frequency < 100, 'emergency alarm uses a low, heavy tone');
assert.ok(sound.cue('motor').gain < sound.cue('alarm').gain, 'unpowered robot ambience stays faint and occasional');
assert.ok(sound.cue('powerRestore').duration >= 0.25, 'full power restoration has a distinct electrical startup cue');
assert.ok(sound.cue('inject').gain < sound.cue('hurtHuman').gain, 'injection remains quieter than an exposed-body hit');
assert.ok(sound.cue('hurtSuit').frequency > sound.cue('hurtHuman').frequency, 'intact armor has a sharper metallic impact');
assert.equal(sound.cue('hurtExposed').notes?.length, 2, 'broken armor mixes suit and body impacts');
assert.equal(sound.cue('hurtHuman').noise?.length, 2, 'exposed-body impact includes a short thud and breath');
assert.notEqual(sound.cue('hurtSuit').type, 'sine', 'armor impact no longer has a pure musical tone');
assert.ok(sound.cue('hurtSuit').notes.every(note => note.duration <= 0.14), 'armor impact is a short clank rather than a sustained note');
assert.ok(sound.cue('hurtSuit').noise[0].frequency > sound.cue('hurtHuman').noise[0].frequency * 3, 'armor crackle and body thud occupy different frequency bands');
assert.ok(sound.cue('hurtExposed').notes.some(note => note.type === 'sawtooth') && sound.cue('hurtExposed').notes.some(note => note.frequency < 200), 'partial armor has a rough crack over a body thud');
assert.ok(sound.cue('swing').noise?.duration > 0, 'a missed swing uses an airy transient instead of a bright plucked note');
assert.ok(sound.cue('wingSiren').frequency > 300, 'research-wing warning stays audible on small speakers');
assert.equal(sound.cue('wingSiren').notes?.length, 2, 'research-wing warning alternates two distinct siren tones');
assert.ok(sound.cue('swing').frequency < 300 && sound.cue('swing').endFrequency < sound.cue('swing').frequency / 2, 'quick attack falls as a rough low whoosh');
assert.ok(sound.cue('chargeReady').frequency < 180, 'finished charge has a heavy low cue');
assert.ok(sound.cue('uiClick').duration < 0.05 && sound.cue('uiClick').noise?.duration < 0.04, 'interaction controls use a dry click instead of a musical note');
assert.ok(sound.cue('impact').frequency <= 140 && sound.cue('impact').endFrequency <= 45, 'a landed hit drops quickly into the bass range');
assert.ok(sound.cue('heavyImpact').gain > sound.cue('impact').gain, 'charged impacts are heavier than regular hits');
assert.ok(sound.cue('impact').noise?.duration > 0, 'a landed hit adds a short noise transient');

const oscillators = [];
const noiseSources = [];
const context = {
  currentTime: 1,
  sampleRate: 8000,
  destination: {},
  createOscillator() {
    const oscillator = { frequency: { value: 0, setValueAtTime(value) { this.value = value; }, exponentialRampToValueAtTime(value) { this.end = value; } }, connect() { return gain; }, start(time) { this.started = time; }, stop(time) { this.stopped = time; } };
    oscillators.push(oscillator);
    return oscillator;
  },
  createGain() {
    gain = { gain: { setValueAtTime() {}, exponentialRampToValueAtTime() {} }, connect(destination) { return destination; } };
    return gain;
  },
  createBuffer(_channels, length) { return { length, getChannelData: () => new Float32Array(length) }; },
  createBufferSource() { const source = { connect(node) { return node; }, start(time) { this.started = time; }, stop(time) { this.stopped = time; } }; noiseSources.push(source); return source; },
  createBiquadFilter() { return { frequency: { value: 0 }, Q: { value: 0 }, connect(node) { return node; } }; }
};
let gain;
const played = sound.play(context, 'swing', () => 0);
assert.ok(played, 'sound cue starts when Web Audio is available');
assert.ok(Math.abs(oscillators[0].frequency.value - sound.cue('swing').frequency * 0.94) < 1e-8, 'each cue applies its randomized pitch');
assert.ok(oscillators[0].stopped > oscillators[0].started, 'sound cue releases its oscillator');
const beforeSiren = oscillators.length;
assert.ok(sound.play(context, 'wingSiren', () => 0.5));
assert.equal(oscillators.length - beforeSiren, 2, 'one siren cue schedules two notes');
assert.ok(oscillators[beforeSiren + 1].started > oscillators[beforeSiren].started, 'the warning alternates instead of stacking notes');
const beforeImpactNoise = noiseSources.length;
assert.ok(sound.play(context, 'impact', () => 0.5));
assert.equal(noiseSources.length - beforeImpactNoise, 1, 'a landed hit plays one short crackle layer');
assert.ok(noiseSources.at(-1).stopped > noiseSources.at(-1).started, 'the noise transient stops automatically');
assert.ok(sound.play(context, 'heavyImpact', () => 0.5));
assert.equal(noiseSources.length - beforeImpactNoise, 2, 'a charged hit also uses a noise transient');
const beforeBodyNoise = noiseSources.length;
assert.ok(sound.play(context, 'hurtHuman', () => 0.5));
assert.equal(noiseSources.length - beforeBodyNoise, 2, 'exposed-body hit schedules both noise layers');
assert.equal(sound.play(null, 'swing'), false, 'missing audio context degrades silently');

console.log('sound cues passed');

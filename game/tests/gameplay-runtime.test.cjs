'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const LastSparkAppearance = require('../player-appearance.js');

const root = path.resolve(__dirname, '..');
const html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
const inlineGame = [...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].at(-1)?.[1];
assert.ok(inlineGame, 'game runtime script exists');

function element() {
  return { style: {}, textContent: '', classList: { toggle() {}, add() {}, remove() {} }, addEventListener() {}, getBoundingClientRect: () => ({ left: 0, top: 0, width: 960, height: 540 }) };
}
const elements = new Map(['#game', '#dialog', '#speaker', '#line', '#advance'].map(id => [id, element()]));
const canvasHandlers = new Map();
elements.get('#game').getContext = () => ({});
elements.get('#game').addEventListener = (type, callback) => canvasHandlers.set(type, callback);
elements.get('#game').dispatchEvent = event => canvasHandlers.get(event.type)?.(event);
const listeners = new Map();
const sandbox = {
  console,
  Math,
  Set,
  URLSearchParams,
  location: { search: '?test=1' },
  document: { querySelector: selector => elements.get(selector) || element() },
  addEventListener(type, callback) { const handlers = listeners.get(type) || []; handlers.push(callback); listeners.set(type, handlers); },
  dispatchEvent(event) { for (const listener of listeners.get(event.type) || []) listener(event); },
  requestAnimationFrame() {},
  KeyboardEvent: class { constructor(type, options = {}) { this.type = type; this.key = options.key; } preventDefault() {} },
  LastSparkAssets: { draw() { return true; } },
  LastSparkAppearance,
  AudioContext: class {
    constructor() { this.currentTime = 0; this.sampleRate = 8; this.destination = {}; this.state = 'running'; }
    createOscillator() { return { frequency: { value: 0 }, connect: node => node, start() {} }; }
    createBiquadFilter() { return { frequency: { value: 0 }, Q: { value: 0 }, connect: node => node }; }
    createGain() { return { gain: { value: 0, setTargetAtTime() {} }, connect: node => node }; }
    createBuffer() { return { getChannelData: () => new Float32Array(8) }; }
    createBufferSource() { return { connect: node => node, start() {} }; }
  },
  LastSparkLighting: { alpha() { return 0; }, flashlightRadius() { return 145; }, sparkIntensity() { return 0; }, glitchIntensity() { return 0; }, draw() {} },
};
sandbox.window = sandbox;
const context = vm.createContext(sandbox);
for (const file of ['combat.js', 'world.js', 'presentation.js', 'lighting.js', 'research.js', 'boss-ai.js', 'sound.js']) {
  vm.runInContext(fs.readFileSync(path.join(root, file), 'utf8'), context, { filename: file });
}
const playedCues = [];
sandbox.LastSparkSound = { play(_context, name) { playedCues.push(name); return true; } };
vm.runInContext(inlineGame, context, { filename: 'index.html gameplay runtime' });
const game = sandbox.LastSparkTest;
sandbox.playLastSparkUiClick();
assert.equal(playedCues.at(-1), 'uiClick', 'a user gesture starts audio and plays the menu click');

game.startRun();
assert.equal(game.snapshot().alarmActive, false, 'the opening corridor has no research alarm');
assert.ok(game.enterResearchWing(false), 'the research wing can be entered for alarm checks');
assert.equal(game.snapshot().mode, 'dialogue', 'the research-wing briefing remains open');
assert.equal(game.snapshot().alarmActive, true, 'the emergency alarm starts before the briefing closes');
assert.equal(playedCues.at(-1), 'wingSiren', 'entering the wing plays the siren immediately');
const sirensAtEntry = playedCues.filter(name => name === 'wingSiren').length;
game.step(0.96);
assert.ok(playedCues.filter(name => name === 'wingSiren').length > sirensAtEntry, 'the siren repeats during the briefing');
game.step(7);
assert.equal(game.snapshot().alarmActive, true, 'the emergency alarm continues while the briefing is open');
game.dismissTestDialogue();
assert.equal(game.snapshot().alarmActive, true, 'the emergency alarm continues when play resumes');
assert.equal(game.snapshot().room, 4, 'research alarm belongs to the research wing');
assert.ok(game.exitResearchWing(), 'the player can leave the research wing');
assert.equal(game.snapshot().alarmActive, false, 'the wing siren stops after leaving the wing');

game.startRun();
playedCues.length = 0;
game.holdKey('j');
game.step(0.66);
assert.equal(playedCues.filter(name => name === 'chargeReady').length, 1, 'the charge threshold produces one low completion cue');
game.step(0.2);
assert.equal(playedCues.filter(name => name === 'chargeReady').length, 1, 'holding the button does not repeat the completion cue');
game.releaseKey('j');

for (const [hp, expectedCue] of [[100, 'hurtSuit'], [50, 'hurtExposed'], [20, 'hurtHuman']]) {
  game.startRun();
  game.setTestPlayerHp(hp);
  playedCues.length = 0;
  game.hurtTestPlayer(2);
  assert.equal(playedCues[0], expectedCue, `the ${hp}% health phase uses its own hit sound`);
}
for (const [hp, damage, expectedPhase, expectedCue] of [[71, 3, 2, 'hurtExposed'], [31, 2, 3, 'hurtHuman']]) {
  game.startRun();
  game.setTestPlayerHp(hp);
  playedCues.length = 0;
  game.hurtTestPlayer(damage);
  assert.equal(game.snapshot().phase, expectedPhase, 'the hit crosses the suit-damage phase threshold');
  assert.equal(playedCues[0], expectedCue, 'the transition hit sounds like the newly exposed phase');
}

game.startRun();
game.setTestPlayerHp(31);
game.hurtTestPlayer(2);
assert.equal(game.snapshot().phase, 3, 'damage can cross the third-phase threshold');
assert.equal(game.snapshot().mode, 'dialogue', 'first entry into phase three pauses for the injection scene');
assert.match(game.snapshot().dialogueText, /주입/, 'the scene explains the experimental injection');
assert.equal(game.snapshot().playerInjected, false, 'the injection takes effect after the scene');
game.dismissTestDialogue();
assert.equal(game.snapshot().playerInjected, true, 'the injection takes effect when the scene closes');
assert.equal(game.snapshot().maxJumps, 3, 'the injection unlocks phase-three triple jump');
game.step(0.5);
game.pressKey(' ');
game.pressKey(' ');
game.pressKey(' ');
assert.equal(game.snapshot().jumps, 3, 'three jumps can be chained after injection');

game.startRun();
game.setTestEnemy(220);
game.step(1);
assert.equal(game.snapshot().alertedEnemies, 0, 'a guard cannot spot the player across an unlit room');
assert.equal(game.snapshot().warningEnemies, 0, 'distant guards do not warn about an unseen player');
game.moveTestPlayerTo(game.snapshot().enemyX[0] - 65);
game.step(0.1);
assert.equal(game.snapshot().warningEnemies, 1, 'a nearby guard displays a warning before pursuit');
assert.equal(game.snapshot().alertedEnemies, 0, 'warning does not immediately become an attack');
game.step(0.9);
assert.equal(game.snapshot().alertedEnemies, 1, 'the guard pursues after the warning interval');

game.setTestTurret();
game.step(3);
assert.equal(game.snapshot().enemyShotCount, 0, 'a turret cannot fire from outside its dark detection range');
game.moveTestPlayerTo(530);
game.step(0.1);
assert.equal(game.snapshot().warningEnemies, 1, 'the turret also warns before aiming');
assert.equal(game.snapshot().enemyShotCount, 0, 'turret warning gives the player time to react');

let state = game.startRun();
assert.equal(state.potions, 2, 'each run starts with exactly two potions');
assert.equal(state.card, false, 'the opening corridor does not grant the security card');
assert.equal(state.objects.some(object => object.key === 'card'), false, 'the card is not positioned in the opening corridor');
game.moveTestPlayerTo(320);
game.pressKey('e');
state = game.snapshot();
assert.equal(state.mode, 'dialogue', 'terminal inspection opens a readable story log');
assert.equal(state.dialogueVisibleChars, 0, 'terminal log begins with an empty typewriter line');
assert.match(state.dialogueText, /메인 AI가 비상 통제 권한을 요청했다/, 'terminal record explains how the AI took control');
game.step(0.5);
state = game.snapshot();
assert.ok(state.dialogueVisibleChars > 0 && state.dialogueVisibleChars < state.dialogueLength, 'terminal log reveals text over time');
game.pressKey('enter');
assert.equal(game.snapshot().dialogueVisibleChars, game.snapshot().dialogueLength, 'continue reveals the complete terminal entry');
game.pressKey('enter');
game.moveTestPlayerTo(1446);
game.pressKey('e');
game.dismissTestDialogue();
state = game.snapshot();
assert.equal(state.room, 1, 'reading the terminal opens the route to the security room');
assert.equal(state.card, false, 'entering the security room does not grant its card automatically');
const openingSecurityCard = state.objects.find(object => object.key === 'card');
assert.ok(openingSecurityCard && openingSecurityCard.x > 900 && openingSecurityCard.label.includes('보안 데스크'), 'the card is placed at the inner security desk');
game.moveTestPlayerTo(1446);
game.pressKey('e');
assert.equal(game.snapshot().room, 1, 'the security bulkhead rejects entry without its card');
game.moveTestPlayerTo(openingSecurityCard.x);
game.pressKey('e');
assert.equal(game.snapshot().card, true, 'the security desk grants the access card');
game.moveTestPlayerTo(1446);
game.pressKey('e');
game.dismissTestDialogue();
assert.equal(game.snapshot().room, 2, 'the security card unlocks the next room');

state = game.startRun();
game.setTestEnemy(500);
game.setTestPlayerHp(55);
game.step(0.5);
assert.equal(game.snapshot().playerHp, 55, 'health does not regenerate with time');
const firstPotion = game.pressKey('r');
assert.equal(firstPotion.playerHp, 65, `one potion restores ten percent max hp; state=${JSON.stringify(firstPotion)}`);
assert.equal(game.snapshot().potions, 1, 'using a potion consumes one charge');
game.pressKey('r');
game.pressKey('r');
assert.equal(game.snapshot().playerHp, 75, 'a run cannot use more than two potions');
assert.equal(game.snapshot().potions, 0, 'potion count stops at zero');

game.startRun();
assert.deepEqual({ phase: game.snapshot().phase, speed: game.snapshot().phaseSpeed, damage: game.snapshot().phaseDamageTakenMultiplier, wear: game.snapshot().suitDamageLevel }, { phase: 1, speed: 150, damage: 0.45, wear: 0 });
assert.equal(game.snapshot().playerAppearance, 'sealed-suit', 'phase one keeps the helmet sealed');
game.setTestPlayerHp(50);
assert.deepEqual({ phase: game.snapshot().phase, speed: game.snapshot().phaseSpeed, damage: game.snapshot().phaseDamageTakenMultiplier, wear: game.snapshot().suitDamageLevel }, { phase: 2, speed: 210, damage: 0.75, wear: 1 });
assert.equal(game.snapshot().playerAppearance, 'visor-breached', 'phase two exposes part of the face through a broken visor');
assert.equal(game.snapshot().playerSprite, 'Player_Evolution_P2', 'phase two swaps to a complete human-and-armor body sprite');
game.setTestPlayerHp(20);
assert.deepEqual({ phase: game.snapshot().phase, speed: game.snapshot().phaseSpeed, damage: game.snapshot().phaseDamageTakenMultiplier, wear: game.snapshot().suitDamageLevel }, { phase: 3, speed: 280, damage: 1.1, wear: 2 });
assert.equal(game.snapshot().playerAppearance, 'human-exposed', 'phase three reveals the person inside the suit');
assert.equal(game.snapshot().playerSprite, 'Player_Evolution_P3', 'phase three uses the human silhouette across the whole body');
game.setTestPlayerHp(100);
assert.equal(game.snapshot().phase, 1, 'recovery can return the health-based movement phase');
assert.equal(game.snapshot().suitDamageLevel, 2, 'armor damage remains after recovery');
assert.equal(game.snapshot().playerAppearance, 'human-exposed', 'recovery does not reseal the damaged suit');
assert.equal(game.snapshot().playerSprite, 'Player_Evolution_P3', 'healing does not restore the sealed suit appearance');
for (const [hp, speed] of [[100, 150], [50, 210], [20, 280]]) {
  game.startRun();
  if (hp < 100) game.setTestPlayerHp(hp);
  const startX = game.snapshot().playerX;
  game.holdKey('d');
  game.step(0.1);
  game.releaseKey('d');
  assert.ok(Math.abs(game.snapshot().playerX - startX - speed * 0.1) < 0.1, `phase ${game.snapshot().phase} moves at its designed speed`);
}
for (const [hp, expected] of [[100, 95.5], [50, 42.5], [20, 9]]) {
  game.startRun();
  if (hp < 100) game.setTestPlayerHp(hp);
  assert.equal(game.hurtTestPlayer(10).playerHp, expected, `phase damage profile applies at ${hp}% health`);
}

game.startRun();
game.setTestPlayerHp(50);
game.holdKey('j');
game.step(0.7);
assert.equal(game.snapshot().playerPose, 'Player_P2_Attack', 'phase two holds its braced attack-ready pose while charging');
game.setTestPlayerHp(20);
assert.equal(game.snapshot().playerPose, 'Player_P3_Attack', 'phase three keeps a braced pose while charging in the damaged suit');
game.releaseKey('j');
assert.equal(game.snapshot().playerPose, 'Player_P3_Attack', 'charged release uses a straight strike instead of a ground slam');

game.startRun();
game.setTestEnemy(500);
game.pressKey('j');
assert.ok(game.snapshot().attackCooldown >= 0.48, 'light attacks have a minimum cadence');
assert.equal(game.snapshot().playerPose, 'Player_P1_Attack', 'a quick tap starts a visible attack animation');
game.holdKey('j');
game.step(0.7);
assert.equal(game.snapshot().playerCharging, true, 'holding attack enters charge state');
assert.ok(game.snapshot().chargeMoveMultiplier < 1, 'charging slows player movement');
assert.equal(game.snapshot().playerPose, 'Player_P1_Attack', 'charging holds a braced pose instead of standing at attention');
assert.equal(game.snapshot().chargeReady, true, 'charge threshold has a clear ready state');
game.releaseKey('j');
assert.equal(game.snapshot().lastAttackCharged, true, 'release after threshold performs charged attack');
assert.equal(game.snapshot().playerPose, 'Player_P1_Attack', 'charged release plays the straight attack animation');
game.step(0.8);
elements.get('#game').dispatchEvent({ type: 'pointerdown', clientX: 400, clientY: 200 });
game.step(0.7);
sandbox.dispatchEvent({ type: 'pointerup' });
assert.equal(game.snapshot().lastAttackCharged, true, 'holding and releasing the mouse also charges an attack');

game.startRun();
game.setTestEnemy(90);
const targetXBeforeHit = game.snapshot().enemyX[0];
game.pressKey('j');
game.step(0.11);
const hitState = game.snapshot();
assert.ok(hitState.hitstopRemaining >= 0.04, 'confirmed melee hit briefly stops the combat simulation');
assert.ok(hitState.cameraShake > 0, 'confirmed hit produces a restrained camera impulse');
assert.ok(hitState.cameraShake <= 3, 'camera impulse stays within the restrained three-pixel limit');
assert.notEqual(hitState.enemyX[0], targetXBeforeHit, 'hit reaction pushes the enemy away');
assert.ok(hitState.hitFeedbackCharged === false, 'basic hit uses the lighter feedback profile');

game.startRun();
game.setTestEnemy(500);
game.hurtTestPlayer(10);
assert.ok(game.snapshot().cameraShake > 0, 'taking damage shakes the camera briefly');
assert.ok(game.snapshot().playerHitFlash > 0, 'taking damage briefly tints the player');
game.step(0.12);
assert.equal(game.snapshot().playerHitFlash, 0, 'player hit tint clears promptly');
assert.equal(game.snapshot().cameraShake, 0, 'damage shake clears after the short impulse');

game.startRun();
game.setTestEnemy(500);
game.holdKey('j');
game.step(0.7);
game.releaseKey('j');
assert.ok(game.snapshot().cameraShake > 0, 'releasing a full charge gives a small camera impulse');

game.startRun();
game.setTestEnemy(500);
game.hurtTestPlayer(10);
game.step(0.5);
game.holdKey('j');
game.step(0.7);
game.releaseKey('j');
assert.ok(game.snapshot().cameraShake <= 1.2, 'a later charge release does not inherit a stronger old hit shake');

game.startRun();
game.setTestEnemy(90);
const basicTargetHp = game.snapshot().enemyHp[0];
game.pressKey('j');
game.step(0.11);
const basicDamage = basicTargetHp-game.snapshot().enemyHp[0];
assert.equal(basicDamage, 9, 'basic strike damage is intentionally restrained');

game.startRun();
game.setTestEnemy(90);
game.holdKey('j');
game.step(0.7);
game.releaseKey('j');
game.step(0.19);
assert.equal(game.snapshot().hitFeedbackCharged, true, 'charged strike uses the stronger impact profile');
assert.ok(58-game.snapshot().enemyHp[0] >= basicDamage*2, 'charged strike deals at least twice the basic damage');

game.startRun();
state = game.pressKey('shift');
assert.ok(state.dashCooldown > 4.9, 'dash is available in the first phase and starts five-second cooldown');
const cooldown = state.dashCooldown;
state = game.pressKey('shift');
assert.equal(state.dashCooldown, cooldown, 'dash cannot be reused before cooldown ends');
game.step(5.1);
assert.ok(game.pressKey('shift').dashCooldown > 4.9, 'dash becomes available again after five seconds');

game.startRun();
assert.ok(game.snapshot().enemyHp.length >= 2, 'opening room has a mixed, more demanding enemy group');
game.enterResearchWing();
game.step(3.2);
assert.ok(game.snapshot().deploymentCount > 0 && game.snapshot().enemyHp.length <= 5, 'research deployment increases pressure without exceeding active enemy cap');

game.setTestBoss('nexus', 1);
const firstPattern = game.triggerBossAttack().bossPattern;
const secondPattern = game.triggerBossAttack().bossPattern;
assert.notEqual(firstPattern, secondPattern, 'boss selects distinct attacks within the same phase');
game.triggerBossAttack();
game.step(1.1);
game.step(0.55);
assert.ok(game.snapshot().bossCounterOpen > 0, 'finishing a boss attack opens a brief counter window');

for (const [kind, phase] of [['sentinel', 1], ['sentinel', 2], ['nexus', 1], ['nexus', 2], ['nexus', 3]]) {
  game.setTestBoss(kind, phase);
  const first = game.triggerBossAttack();
  const second = game.triggerBossAttack();
  assert.notEqual(first.bossPattern, second.bossPattern, `${kind} phase ${phase} has two distinguishable attack patterns`);
  if (kind === 'nexus' && phase === 3) {
    assert.equal(first.bossHazardKind, 'split', 'final phase first marks a safe gap');
    assert.equal(second.bossHazardKind, 'ring', 'final phase alternate uses a broad core pulse');
  }
}

game.setTestBoss('nexus', 2, 300);
const startingBossX = game.snapshot().bossX;
game.step(0.5);
assert.ok(game.snapshot().bossX < startingBossX, 'final boss advances toward the player between attacks');
for (const phase of [1, 2, 3]) {
  game.setTestBoss('nexus', phase, 300);
  const ids = new Set();
  const count = phase === 3 ? 4 : 3;
  for (let i = 0; i < count; i++) ids.add(game.triggerBossAttack().bossPattern);
  assert.equal(ids.size, count, `final boss phase ${phase} offers ${count} distinct moves`);
  assert.ok([...ids].some(id => id.includes('rush')), 'each phase includes a telegraphed moving attack');
}
game.setTestBoss('nexus', 3, 300);
game.triggerBossAttack();
game.triggerBossAttack();
const rush = game.triggerBossAttack();
assert.equal(rush.bossHazardKind, 'rush', 'final phase warns before a body rush');
assert.ok(rush.bossHazardWarn >= 0.65, 'rush has a visible reaction window');
const beforeRushX = rush.bossX;
game.step(rush.bossHazardWarn);
game.step(0.2);
assert.notEqual(game.snapshot().bossX, beforeRushX, 'NEXUS physically crosses the arena during its rush');
game.setTestBoss('nexus', 3, 300);
for (let i = 0; i < 4; i++) game.triggerBossAttack();
game.step(0.8);
assert.ok(game.snapshot().bossShotCount >= 3, 'the final-phase volley releases a spread of projectiles');

game.startRun();
let roomObjects = game.snapshot().objects;
assert.ok(!roomObjects.some(object => object.key === 'card'), 'security card is no longer left in the entry hall');
assert.ok(roomObjects.some(object => object.key === 'door'), 'entry hall retains the route to the security room');
assert.equal(typeof game.moveTestPlayerTo, 'function', 'runtime test can position the player for interaction checks');
assert.equal(typeof game.dismissTestDialogue, 'function', 'runtime test can dismiss room-entry dialogue');
game.moveTestPlayerTo(320);
game.pressKey('e');
game.moveTestPlayerTo(1446);
game.dismissTestDialogue();
game.pressKey('e');
game.dismissTestDialogue();
assert.equal(game.snapshot().room, 1, 'player enters the dedicated security room before obtaining the card');
roomObjects = game.snapshot().objects;
const securityCard = roomObjects.find(object => object.key === 'card');
assert.ok(securityCard && securityCard.x > 900, 'card sits at the security desk beyond the room midpoint');
assert.ok(roomObjects.some(object => object.key === 'return'), 'security room keeps a route back to the research wing');
game.moveTestPlayerTo(1446);
game.pressKey('e');
assert.equal(game.snapshot().room, 1, 'security bulkhead stays locked until the card is collected');
game.moveTestPlayerTo(securityCard.x);
game.pressKey('e');
assert.equal(game.snapshot().card, true, 'player collects the card at the security desk');
game.moveTestPlayerTo(70);
game.pressKey('e');
assert.equal(game.snapshot().room, 0, 'player can backtrack from security to the optional power wing');

game.startRun();
const unpoweredMix = game.snapshot().audioLevels;
assert.equal(game.snapshot().flashlightOn, true, 'flashlight starts enabled');
assert.equal(game.pressKey('o').flashlightOn, false, 'O toggles the flashlight off');
assert.equal(game.pressKey('o').flashlightOn, true, 'O toggles the flashlight back on');
assert.equal(game.interactGenerator('g1'), true, 'test activates the first research-wing generator');
game.step(0.25);
const poweredState = game.snapshot();
assert.ok(poweredState.audioLevels.noise < unpoweredMix.noise, 'generator activation reduces ambient hiss');
assert.ok(poweredState.audioLevels.drone < unpoweredMix.drone, 'generator activation reduces the low drone');
assert.ok(poweredState.alarmPulseUntil > 0, 'generator activation triggers a short emergency-light pulse');
assert.equal(poweredState.alarmActive, true, 'generator alarm is active during its pulse window');
assert.ok(poweredState.warningEnemies > 0, 'emergency alarm warns nearby robots before pursuit');
assert.equal(poweredState.alertedEnemies, 0, 'the generator alarm leaves a reaction window before enemy attacks');
assert.equal(poweredState.flashlightOn, true, 'the flashlight stays available while power is incomplete');
game.interactGenerator('g2');
game.interactGenerator('g3');
assert.equal(game.snapshot().powerLevel, 3, 'all three generators restore full power');
assert.equal(game.snapshot().flashlightOn, false, 'full power automatically switches the flashlight off');
assert.equal(game.snapshot().darkness, 0.02, 'full power leaves only a near-zero dimming layer across the map');
assert.equal(game.snapshot().flashlightRadius, 145, 'a narrow manual flashlight remains available after restoration');
assert.equal(game.pressKey('o').flashlightOn, true, 'O can manually restore the flashlight after its automatic power-up shutdown');

console.log('PASS: gameplay runtime checks');

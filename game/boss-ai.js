(function (root) {
  'use strict';

  const clamp = (value, min, max) => Math.max(min, Math.min(max, value));

  function move(boss, player, dt, width, hazard = null) {
    if (boss.kind !== 'nexus' || boss.counterOpen > 0 || (hazard && (hazard.kind === 'rush' || hazard.warn <= 0))) return boss.x;
    const margin = 135;
    const side = boss.moveSide || (boss.x >= player.x ? 1 : -1);
    boss.moveSide = side;
    const target = clamp(player.x + side * 135, margin, width - margin);
    const speed = [85, 120, 165][Math.max(0, Math.min(2, boss.phase - 1))] * (hazard ? 0.55 : 1);
    boss.x = clamp(boss.x + clamp(target - boss.x, -speed * dt, speed * dt), margin, width - margin);
    boss.facing = player.x < boss.x ? -1 : 1;
    return boss.x;
  }

  function nextPattern(boss, count, random = Math.random) {
    if (!Array.isArray(boss.patternQueue) || boss.patternQueue.length === 0 || boss.patternPhase !== boss.phase) {
      const queue = Array.from({ length: count }, (_, index) => index);
      if (boss.patternPhase === boss.phase) {
        for (let i = queue.length - 1; i > 0; i--) {
          const j = Math.floor(random() * (i + 1));
          [queue[i], queue[j]] = [queue[j], queue[i]];
        }
      }
      if (queue[0] === boss.lastPattern && queue.length > 1) {
        const other = queue.findIndex(pattern => pattern !== boss.lastPattern);
        [queue[0], queue[other]] = [queue[other], queue[0]];
      }
      boss.patternQueue = queue;
      boss.patternPhase = boss.phase;
    }
    boss.lastPattern = boss.patternQueue.shift();
    return boss.lastPattern;
  }

  function rushTarget(boss, player, width) {
    const margin = 135;
    let direction = player.x >= boss.x ? 1 : -1;
    if (boss.x + direction * 220 < margin || boss.x + direction * 220 > width - margin) direction *= -1;
    const distance = Math.max(220, Math.min(400, Math.abs(player.x - boss.x) + 70));
    return clamp(boss.x + direction * distance, margin, width - margin);
  }

  const api = Object.freeze({ move, nextPattern, rushTarget });
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else root.LastSparkBossAI = api;
})(typeof window !== 'undefined' ? window : globalThis);

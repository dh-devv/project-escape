(() => {
  'use strict';

  const clampPhase = phase => Math.max(1, Math.min(3, Math.floor(Number(phase) || 1)));

  function selectPlayerVisual(phase, state = {}) {
    const wear = Math.max(0, Math.min(2, Math.floor(Number(state.suitDamageLevel) || 0)));
    const tier = clampPhase(Math.max(Number(phase) || 1, wear + 1));
    if (tier === 1) return { tier: 1, name: 'Player_P1', frame: null, appearance: 'sealed-suit' };

    let frame;
    if (state.dead) frame = 8;
    else if (state.hurt || state.stun) frame = 7;
    else if (state.charging) frame = 4;
    else if (state.attack) {
      const progress = Math.max(0, Math.min(1, Number(state.attackProgress) || 0));
      frame = progress < 0.22 ? 4 : progress < 0.72 ? 5 : 8;
    } else if (state.airborne) frame = 6;
    else if (state.moving) frame = 2 + (Math.floor(Math.max(0, Number(state.elapsed) || 0) * 8) % 2);
    else frame = Math.floor(Math.max(0, Number(state.elapsed) || 0) * 1.7) % 2;

    return {
      tier,
      name: `Player_Evolution_P${tier}`,
      frame,
      appearance: tier === 2 ? 'visor-breached' : 'human-exposed'
    };
  }

  const api = Object.freeze({ selectPlayerVisual });
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  if (typeof window !== 'undefined') window.LastSparkAppearance = api;
})();
